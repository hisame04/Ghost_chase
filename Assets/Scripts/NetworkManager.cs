////*ゲーム内の通信制御用のスクリプト*////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;//どこからでもアクセスできるように

    public ServerGameState ServerState { get; private set; }

    //自身のIPアドレス情報
    string myLocalIp;
    string myGlobalIp;
    //サーバを立てるときに使用するポート
    string serverSelectIP;
    int serverPort = 8000;

    //ルームへの接続
    private int limit_roomIdLength = 4;

    //ソケット通信用
    private UdpGameServer _udpServer;
    //private TcpGameServer _tcpServer; //【TCP導入後挿入箇所】
    private IGameClient _client;
    DatabaseReference reference;

    //ホストとクライアントの受信処理のハンドル
    private HostPacketHandler _hostHandler;
    private ClientPacketHandler _clientHandler;


    // キャンセル用のトークンソース
    private CancellationTokenSource _cts;

    //他のスクリプトに通知を送るためのイベント
    public event Action OnJoinRoomSuccess;//部屋接続成功時
    public event Action<string> OnJoinRoomFailed;//部屋接続失敗時
    public event Action OnMemberListUpdate;//部屋のメンバー更新時
    public event Action<bool> OnHostAllReady;//全員が準備完了した時
    public event Action<bool> OnTagPlayerChanged;//鬼かどうかが変わった時

    private ConcurrentQueue<string> _packetQueue = new ConcurrentQueue<string>();


    ////*Awake*////
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _cts = new CancellationTokenSource();

            _hostHandler = new HostPacketHandler(this);//ホストのハンドラー
            _clientHandler = new ClientPacketHandler(this);//クライアントのハンドラー

            ServerState = new ServerGameState();
            DontDestroyOnLoad(gameObject); // シーン遷移しても消えないようにする
        }
        else
        {
            Destroy(gameObject); // 重複して存在しないようにする
            return;
        }
    }

    ////*Start*////
    async void Start()
    {
        //部屋の最大人数の設定
        if(ServerState != null && GameSessionData.Instance != null)
        {
            Debug.Log($"ServerState.maxPlayerNum: {GameSessionData.Instance.playerLimitNum}");
            ServerState.maxPlayerNum = GameSessionData.Instance.playerLimitNum;
        }

        try
        { // Firebaseの準備
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            //IPアドレス情報の取得
            await GetMyIP();

            //プロトコルの初期設定
            GameSessionData.Instance.SetProtcol((int)GameSessionData.ProtocolType.UDP);
        }
        catch (Exception ex)
        {
            Debug.Log($"IPアドレス取得エラー{ex.Message}");
        }
    }

    ////*Update*////
    void Update()
    {
        //
        while (_packetQueue.TryDequeue(out string json))
        {
            _clientHandler.Handle(json);
        }

        // テスト用: スペースキーを押したら送信
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_client != null)
            {
                _client.SendMessage($"Test Message {Time.time}");
                Debug.Log("テストメッセージ送信");
            }
        }
    }

    ////*アプリケーション終了時の処理*////
    /*Unityのオブジェクト破壊時の処理*/
    void OnDestroy()
    {
        // 「全てのタスク中止！」という命令を出す
        _cts?.Cancel();
        _cts?.Dispose();

        if (GameSessionData.Instance != null &&
        GameSessionData.Instance.selectedRole == GameSessionData.RoleType.Host &&
        !string.IsNullOrEmpty(GameSessionData.Instance.roomId))
        {
            // レファレンスが生きていれば削除を実行
            if (reference != null)
            {
                string protocolName = GetProtocolName();
                reference.Child("rooms").Child(protocolName).Child(GameSessionData.Instance.roomId).RemoveValueAsync();
            }
        }
    }

    /*自身のIPアドレスを取得する*/
    async Task GetMyIP()
    {
        myLocalIp = NetworkUtils.GetLocalIPAddress();
        Debug.Log(myLocalIp);
        myGlobalIp = await NetworkUtils.GetGlobalIPAdress();
        Debug.Log(myGlobalIp);
    }

    ////*ボタンから呼び出すメソッド*////
    //*UDP選択時の処理*//
    public void OnSelectUDP()
    {
        GameSessionData.Instance.SetProtcol((int)GameSessionData.ProtocolType.UDP);
    }

    //*TCP選択時の処理*//
    public void OnSelectTCP()
    {
        GameSessionData.Instance.SetProtcol((int)GameSessionData.ProtocolType.TCP);
    }

    //*ホスト選択時の処理*//
    public async Task<string> OnHostSelect()
    {
        //選択したロールをデータにセット
        GameSessionData.Instance.SetRole((int)GameSessionData.RoleType.Host);
        //部屋番号決める(ランダム生成→チェック→チェックが通ったら決定&確保)
        string roomId = await AutSetRoomId();
        ////失敗した場合の処理
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.Log("【ホスト】部屋番号の生成に失敗");
            return null;
        }
        //書き込んだIPアドレスでサーバ起動
        bool isRoomCreated = await CreateRoom(roomId);
        ////失敗した場合の処理
        if (!isRoomCreated)
        {
            Debug.LogError("【ホスト】部屋作成処理に失敗");
            return null;
        }
        bool isStartHostClient = await StartHostClient();
        ////失敗した場合の処理
        if (!isStartHostClient)
        {
            Debug.Log("【ホスト】ホスト自身の接続に失敗");
            return null;
        }

        Debug.Log("【ホスト】セットアップ完了");
        return roomId;
    }

    //*クライアント選択時の処理*//
    public void OnClientSelect()
    {
        //選択したロールをデータにセット
        GameSessionData.Instance.SetRole((int)GameSessionData.RoleType.Client);
        //クライアントの生成
        SetClient((int)GameSessionData.Instance.selectedProtocol);
    }

    //*部屋番号入力決定時*//
    public async Task<string> OnDecideRoomId(string roomId)
    {
        if (!CheckDecideRoomId(roomId))
        {
            Debug.LogError("部屋番号は4桁の数字で入力してください！");
            return null;
        }

        //連打防止
        // roomIdDecideButton.interactable = false;

        //入力された部屋番号のホストへ接続
        if (await ConnectServer(roomId))
        {
            var connectData = new ConnectRequestData { playerName = GameSessionData.Instance.myName };
            SendPacketToHost(NetworkCommand.ConnectRequest, connectData);


            //部屋番号をGameSessionDataに登録
            GameSessionData.Instance.SetRoomId(roomId);

            Debug.Log("【クライアント】ホストへの接続完了");
            await Task.Delay(10);
            
            return roomId;
        }
        else
        {
            Debug.Log("ホストの接続に失敗しました");
            return null;
        }

    }
    ////////////////////////////////


    ////*その他メソッド*////

    /*選択しているプロトコル名をstringで取得*/
    private string GetProtocolName()
    {
        return GameSessionData.Instance.selectedProtocol.ToString(); // "UDP" または "TCP" を返す
    }


    /*ホストのクライアントの接続*/
    private async Task<bool> StartHostClient()
    {
        try
        {
            // クライアント機能の生成
            SetClient((int)GameSessionData.Instance.selectedProtocol);
            // 自分自身(localhost)に接続
            await _client.ConnectAsync("127.0.0.1", serverPort);
            //自分自身に接続要求
            var connectData = new ConnectRequestData { playerName = GameSessionData.Instance.myName };
            SendPacketToHost(NetworkCommand.ConnectRequest, connectData);
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"サーバ接続エラー{ex.Message}");
            return false;
        }
    }

    /*部屋番号のチェック*/
    private async Task<bool> CheckRoomId(string roomId)
    {
        string myToken = Guid.NewGuid().ToString();

        string protocolName = GetProtocolName();
        DatabaseReference target = reference.Child("rooms").Child(protocolName).Child(roomId.ToString());
        DataSnapshot snapshot = await target.RunTransaction(mutableData =>
        {
            // もしすでにデータがあったら（誰かが使っていたら）
            if (mutableData.Value != null)
            {
                // 処理を中止（Abort）する
                return TransactionResult.Abort();
            }

            //もしデータがなかったら
            mutableData.Value = myToken;//仮の文字を書き込む
            return TransactionResult.Success(mutableData);//処理を完了する
        });

        return snapshot.Value != null && snapshot.Value.ToString() == myToken;
    }

    /*部屋番号のランダム生成*/
    private string RandomRoomIdSet()
    {
        // 1000〜9999の間でランダムな数字を作る
        string randomId = UnityEngine.Random.Range(1000, 9999).ToString();
        return randomId;
    }

    /*部屋番号の重複防止生成*/
    private async Task<string> AutSetRoomId()
    {
        if (_cts == null || IsCtsDisposed(_cts))
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }
        Debug.Log("部屋番号生成中...");
        while (Application.isPlaying)
        {
            if (_cts.Token.IsCancellationRequested) return null;//キャンセル命令が出ていたら処理を打ち切る

            //部屋番号をランダムで生成
            string roomId = RandomRoomIdSet();

            try
            {
                //生成した部屋番号が使用可能(使われていないか)チェック
                bool isAvailable = await CheckRoomId(roomId);

                if (isAvailable)
                {
                    return roomId;//使われていなかったらその部屋番号を返す
                }

                await Task.Delay(100, _cts.Token);
            }
            catch (TaskCanceledException)//停止処理がされた場合
            {
                return null;
            }
        }
        return null;
    }

    /*部屋作成(サーバ作成)*/
    async Task<bool> CreateRoom(string roomId)
    {
        RoomData data = new RoomData(myLocalIp, myGlobalIp, serverPort);//自分のIPアドレス情報をまとめる
        string json = JsonUtility.ToJson(data);//まとめたIPアドレス情報をjson形式にする

        try
        {
            string protocolName = GetProtocolName();
            var roomRef = reference.Child("rooms").Child(protocolName).Child(roomId);
            //切断された際の処理
            await roomRef.OnDisconnect().RemoveValue();
            //firebaseへjson形式にしたIPアドレス情報の書き込み
            await roomRef.SetRawJsonValueAsync(json);

            GameSessionData.Instance.roomId = roomId;
            //サーバの起動
            SetServer((int)GameSessionData.Instance.selectedProtocol);

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"部屋作成エラー{ex.Message}");
            return false;
        }
    }

    /*サーバの生成・開始*/
    private async void SetServer(int protcol)
    {
        GameSessionData.ProtocolType type = (GameSessionData.ProtocolType)protcol;
        if (type == GameSessionData.ProtocolType.UDP)
        {
            _udpServer = new UdpGameServer();

            //OnReceiveで受信したらHostPacketHandlerのHandleを呼び出す
            _udpServer.OnDataReceived += (data, remote) =>
            {
                _hostHandler.Handle(data, remote);
            };

            await _udpServer.StartServer(serverPort);
            Debug.Log("Create UDPServer");
        }
        else if (type == GameSessionData.ProtocolType.TCP)
        {
            //_tcpServer = new TcpGameServer(); //【TCP導入後挿入箇所】
            //_tcpServer.OnDataReceived += (data,remote) =>{_hostHandler.Handle(data,remote)}
            //await _tcpServer.StartServer(serverPort); //【TCP導入後挿入箇所】
            Debug.Log("Create TCPServer");
        }
    }

    /*クライアントの選択・生成*/
    private void SetClient(int protcol)
    {
        GameSessionData.ProtocolType type = (GameSessionData.ProtocolType)protcol;
        if (type == GameSessionData.ProtocolType.UDP)
        {
            _client = new UdpGameClient();

            //OnReceiveで受信したらClientPacketHandlerのHandleを呼び出す
            _client.OnDataReceived += (data) =>
            {
                // _clientHandler.Handle(data);
                _packetQueue.Enqueue(data);
            };

        }
        else if (type == GameSessionData.ProtocolType.TCP)
        {
            //_client = new TcpGameClient(); //【TCP導入後挿入箇所】
        }
    }

    /*firebaseからサーバのIPアドレスの取得*/
    private async Task<DataSnapshot> GetServerIP(string roomId)
    {
        string protocolName = GetProtocolName();
        DatabaseReference target = reference.Child("rooms").Child(protocolName).Child(roomId);

        var timeoutTask = Task.Delay(5000);
        var getValueTask = target.GetValueAsync();

        var completedTask = await Task.WhenAny(getValueTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            Debug.LogError("タイムアウト: 部屋情報の取得に時間がかかりすぎました");
            return null;
        }

        DataSnapshot snapshot = await getValueTask;

        if (!snapshot.Exists)
        {
            Debug.LogError("指定された部屋は見つかりません");
            return null;
        }

        return snapshot;
    }

    /*自身のアドレスと比較した接続先のIPアドレスの選択*/
    private async Task<string> SelectServerIP(string roomId)
    {
        string selectServerIP;

        //firebaseからroomIdのデータを読み取る
        DataSnapshot snapshot = await GetServerIP(roomId);

        //部屋が見つからなかった場合
        if (snapshot == null || !snapshot.Exists)
        {
            Debug.Log("部屋が見つかりません");
            return null;
        }

        //読み取ったデータを変数に代入
        string hostLocalIP = snapshot.Child("localIp").Value.ToString();
        string hostGlobalIp = snapshot.Child("globalIp").Value.ToString();

        //同じコンピュータの場合
        if (myLocalIp == hostLocalIP && myGlobalIp == hostGlobalIp)
        {
            Debug.Log("ホストIP判定: 自分自身(Localhost)");
            selectServerIP = "127.0.0.1";
        }
        //ローカルネットワーク内にホストがいた場合
        else if (myGlobalIp == hostGlobalIp)
        {
            Debug.Log("ホストIP判定: 同じネットワーク内");
            selectServerIP = hostLocalIP;
        }
        //ローカルネットワーク内にホストがいない場合
        else
        {
            Debug.Log("ホストIP判定: 異なるネットワーク");
            selectServerIP = hostGlobalIp;
        }

        return selectServerIP;
    }

    /*入力が部屋番号の形式に当てはまっているかチェック*/
    private bool CheckDecideRoomId(string input)
    {
        //長さが4(変数指定)、数字の入力、1000以上9999以下の場合true
        return input.Length == limit_roomIdLength && input.All(char.IsDigit) && 1000 <= int.Parse(input) && int.Parse(input) <= 9999;
    }

    /*サーバへの接続*/
    private async Task<bool> ConnectServer(string roomId)
    {
        try
        {
            string serverSelectIP = await SelectServerIP(roomId);
            //IPアドレスの取得に失敗した場合
            if (string.IsNullOrEmpty(serverSelectIP))
            {
                Debug.LogError("IPアドレスの取得に失敗しました");
                return false;
            }
            //取得できたら、サーバに接続する
            await _client.ConnectAsync(serverSelectIP, serverPort);
            return true;//接続できたらtrueを返す
        }
        catch (Exception ex)//エラー処理
        {
            Debug.LogError($"接続失敗: {ex.Message}");
            return false;
        }
    }

    //*ホストにアバター情報を送る*//
    public void SendAvatarDataToHost()
    {
        var avatarData = new SendAvatarData(GameSessionData.Instance.myIDNum,GameSessionData.Instance.isReady,GameSessionData.Instance.myAvatarIndex,GameSessionData.Instance.myColorIndex);
        SendPacketToHost(NetworkCommand.SendAvatarData,avatarData);
        Debug.Log($"[Client]　アバター情報送信 id:{avatarData.id} isReady:{avatarData.isReady}");
    }

    //*部屋接続成功時*//
    public void TriggerJoinRoomSuccess()
    {
        OnJoinRoomSuccess?.Invoke();
    }

    //*部屋接続失敗時*//
    public void TriggerJoinRoomFailed(string reason)
    {
        DisconnectLocal();
        OnJoinRoomFailed?.Invoke(reason);
    }

    //*部屋のメンバー更新時*//
    public void TriggerMemberListUpdate()
    {
        OnMemberListUpdate?.Invoke();
    }

    //*全員準備完了になった時*//
    public void TriggerHostAllReady(bool isAllReady)
    {
        OnHostAllReady?.Invoke(isAllReady);
    }

    //*鬼交代が起こった時*//
    public void TriggerTagChange(bool isTag)
    {
        OnTagPlayerChanged?.Invoke(isTag);
    }

    //*ゲーム開始時*//
    public void OnGameStart()
    {
        ServerState.DecideInitialGameData();//初期設定項目の設定

        //ホストのマスターデータから配布用のデータを作成
        List<GameStartPlayerData> playerList = new List<GameStartPlayerData>();
        foreach(var player in ServerState.MasterPlayers)
        {
            var profile = player.Value.playerProfile;
            GameStartPlayerData playerData = new GameStartPlayerData(profile.id,profile.avatarIdx,profile.colorIdx,profile.firstSpawnPos,profile.isTag);

            playerList.Add(playerData);
        }

        //ステージの送信(ステージを増やした時ここで変更)
        string stage = "Stage1";

        //作成したリストを全クライアントに送信
        GameStartPlayerDataList initialData = new GameStartPlayerDataList(playerList,stage);
        BroadPacketToAllClient(NetworkCommand.GameStart,initialData);

        StartCoroutine(AnnouncePlayerPos());//位置情報のクライアントへの一斉送信コルーチンの開始
    }

    //*位置情報一斉送信のループのためのコルーチン*//
    public IEnumerator AnnouncePlayerPos()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(_udpServer != null && ServerState.MasterPlayers.Count > 0)//【TCP実装時】tcpServerも追加
            {
                SendAllPlayerPos();
            }
        }
    }

    //*位置情報更新メソッド*//
    private void SendAllPlayerPos()
    {
        List<MoveStateData> allMoveList = new List<MoveStateData>();//プレイヤー位置情報のリストの作成
        foreach(var player in ServerState.MasterPlayers)
        {
            MoveStateData data = new MoveStateData
            {
                id = player.Value.playerProfile.id,
                pos = player.Value.playerProfile.currentPos,
                rot = player.Value.playerProfile.currentRot
            };
            allMoveList.Add(data);
        }

        MoveAnnounceData announceData = new MoveAnnounceData{moveStateList = allMoveList};//送信用のデータの作成
        BroadPacketToAllClient(NetworkCommand.UpdatePos,announceData);//全クライアントへの一斉送信
    }

    /*部屋離脱メソッド*/
    public void LeaveRoom()
    {
        //クライアントが離脱した場合
        if(GameSessionData.Instance.selectedRole == GameSessionData.RoleType.Client)
        {
            SendPacketToHost(NetworkCommand.Disconnect, new DisconnectData{playerId = GameSessionData.Instance.myIDNum});
        }
        //ホストが離脱した場合
        else
        {
            
        }

        //通信を切断する
        DisconnectLocal();

        //ロール選択画面に戻る
        
    }

    /*通信切断用のメソッド*/
    public void DisconnectLocal()
    {
        // キャンセルトークンを止める
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        // UDPクライアントを閉じる
        if (_client != null)
        {
            _client.Close();
            _client = null;
        }

        // サーバー機能も動いていれば閉じる（ホストの場合）
        if (_udpServer != null)
        {
            _udpServer.Stop();
            _udpServer = null;
        }

        Debug.Log("通信を切断しました");
    }

    /*キャンセルトークンが存在するかの確認メソッド*/
    private bool IsCtsDisposed(CancellationTokenSource cts)
    {
        try
        {
            // Tokenにアクセスして例外が出たら破棄されている
            var t = cts.Token;
            return t.IsCancellationRequested; // キャンセル済みの場合も作り直したいので
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    /*ホストにパケットを送信するためのメソッド*/
    public void SendPacketToHost(NetworkCommand command, object dataObj)
    {
        //中身をJSON化
        string bodyJson = JsonUtility.ToJson(dataObj);

        //パケット作成
        GamePacket packet = new GamePacket();
        packet.command = command;
        packet.dataJson = bodyJson;

        //パケットをJSON化して送信
        string finalJson = JsonUtility.ToJson(packet);

        if (_client != null)
        {
            _client.SendMessage(finalJson);
        }
    }

    /*特定のクライアントにパケットを送信するためのメソッド*/
    public void SendPacketToClient(NetworkCommand command, object dataObj, IPEndPoint remote)
    {
        //中身をJSON化
        string bodyJson = JsonUtility.ToJson(dataObj);

        //パケット作成
        GamePacket packet = new GamePacket();
        packet.command = command;
        packet.dataJson = bodyJson;

        //パケットをJSON化して送信
        string finalJson = JsonUtility.ToJson(packet);

        //UDPを選択した場合
        if (GameSessionData.Instance.selectedProtocol == GameSessionData.ProtocolType.UDP)
        {
            if (_udpServer != null)
            {
                _udpServer.SendMessage(finalJson, remote);
            }
        }
        //TCPを選択した場合
        else if (GameSessionData.Instance.selectedProtocol == GameSessionData.ProtocolType.TCP)
        {
            // if(_tcpServer != null)
            // {
            //     _tcpServer.SendMessage(finalJson, remote);
            // }
        }
    }

    public void BroadPacketToAllClient(NetworkCommand command, object dataObj)
    {
        //中身をJSON化
        string bodyJson = JsonUtility.ToJson(dataObj);

        //パケット作成
        GamePacket packet = new GamePacket();
        packet.command = command;
        packet.dataJson = bodyJson;

        //パケットをJSON化して送信
        string finalJson = JsonUtility.ToJson(packet);

        //UDPを選択した場合
        if (GameSessionData.Instance.selectedProtocol == GameSessionData.ProtocolType.UDP)
        {
            if (_udpServer != null)
            {
                foreach(var player in ServerState.MasterPlayers)
                {
                    _udpServer.SendMessage(finalJson, player.Value.clientIP);
                    Debug.Log($"一斉送信：to {player.Value.clientIP}");
                }
            }
        }
        //TCPを選択した場合
        else if (GameSessionData.Instance.selectedProtocol == GameSessionData.ProtocolType.TCP)
        {
            // if(_tcpServer != null)
            // {
                // foreach(var player in ServerState.MasterPlayers)
                // {
                //     _tcpServer.SendMessage(finalJson, remote);
                // }
            // }
        }
    }
}
