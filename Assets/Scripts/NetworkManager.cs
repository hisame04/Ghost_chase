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

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;//どこからでもアクセスできるように

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

    //UI用変数
    public GameObject protocolPanel;
    public GameObject rolePanel;
    public GameObject roomConnectPanel_client;
    public GameObject selectAvatarPanel;

    public Button udpButton;//UDP選択用ボタン
    public Button tcpButton;//TCP選択用ボタン
    public Button hostButton;//部屋を作成するのを選択する(ホスト)用ボタン
    public Button clientButton;//部屋に入るのを選択する(クライアント用)ボタン
    public Button roomIdDecideButton;//部屋番号を指定して決定するボタン
    public TMP_InputField roomIdInput;//部屋番号を入力する欄
    public TMP_InputField roomIdShow;//部屋番号を入力する欄

    DatabaseReference reference;

    // キャンセル用のトークンソース
    private CancellationTokenSource _cts;

////*Awake*////
    void Awake()
    {
        Instance = this;

        //UIの表示非表示の設定
        protocolPanel.SetActive(true);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(false);

        _cts = new CancellationTokenSource();
    }

////*Start*////
    async void Start()
    {
        try
        { // Firebaseの準備
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            //IPアドレス情報の取得
            await GetMyIP();

            //UIのボタンのセット
            udpButton.onClick.AddListener(OnSelectUDP);
            tcpButton.onClick.AddListener(OnSelectTCP);
            hostButton.onClick.AddListener(OnHostStart);
            clientButton.onClick.AddListener(OnClientStart);
            roomIdDecideButton.onClick.AddListener(OnDecideRoomId);

        }
        catch (Exception ex)
        {
            Debug.Log($"IPアドレス取得エラー{ex.Message}");
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
                reference.Child("rooms").Child(GameSessionData.Instance.roomId).RemoveValueAsync();
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

////*ボタンに割り当てるメソッド*////
    //*UDP選択時の処理*//
    public void OnSelectUDP() => SetProtcol((int)GameSessionData.ProtocolType.UDP);

    //*TCP選択時の処理*//
    public void OnSelectTCP() => SetProtcol((int)GameSessionData.ProtocolType.TCP);

    //*ホスト選択時の処理*//
    private async void OnHostStart()
    {
        //選択したロールをデータにセット
        SetRole((int)GameSessionData.RoleType.Host);
        //部屋番号決める(ランダム生成→チェック→チェックが通ったら決定&確保)
        string roomId = await AutSetRoomId();
        ////失敗した場合の処理
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.Log("【ホスト】部屋番号の生成に失敗");
            SetButtonsInteractable(true);
            return;
        }
        //書き込んだIPアドレスでサーバ起動
        bool isRoomCreated = await CreateRoom(roomId);
        ////失敗した場合の処理
        if (!isRoomCreated)
        {
            Debug.LogError("【ホスト】部屋作成処理に失敗");
            SetButtonsInteractable(true);
            return;
        }
        bool isStartHostClient = await StartHostClient();
        ////失敗した場合の処理
        if (!isRoomCreated)
        {
            Debug.Log("【ホスト】ホスト自身の接続に失敗");
            SetButtonsInteractable(true);
            return;
        }

        //決定した部屋番号を表示する
        SetRoomIdShow(roomIdShow, roomId);

        //UIの表示非表示の設定
        protocolPanel.SetActive(false);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(true);

        Debug.Log("【ホスト】セットアップ完了");
    }

    //*クライアント選択時の処理*//
    private async void OnClientStart()
    {
        //選択したロールをデータにセット
        SetRole((int)GameSessionData.RoleType.Client);
        //クライアントの生成
        SetClient((int)GameSessionData.Instance.selectedProtocol);

        //UIの表示非表示の設定
        protocolPanel.SetActive(false);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(true);
        selectAvatarPanel.SetActive(false);
    }

    //*部屋番号入力決定時*//
    public async void OnDecideRoomId()
    {
        string roomId = roomIdInput.text;
        if (!CheckDecideRoomId(roomId))
        {
            Debug.LogError("部屋番号は4桁の数字で入力してください！");
            // ここでユーザーに警告ポップアップなどを出すと親切です
            roomIdInput.text = null;
            return;
        }
        else
        {
            //入力された部屋番号のホストへ接続
            if (await ConnectServer(roomId))
            {
                //部屋番号をGameSessionDataに登録
                SetRoomId(roomId);

                //UIの表示非表示の設定
                protocolPanel.SetActive(false);
                rolePanel.SetActive(false);
                roomConnectPanel_client.SetActive(false);
                selectAvatarPanel.SetActive(true);
                Debug.Log("【クライアント】ホストへの接続完了");
            }
            else
            {
                Debug.Log("ホストの接続に失敗しました");
            }

        }
    }
////////////////////////////////
    

////*その他メソッド*////
    /*ホストのクライアントの接続*/
    private async Task<bool> StartHostClient()
    {
        try
        {
            // クライアント機能の生成
            SetClient((int)GameSessionData.Instance.selectedProtocol);
            // 自分自身(localhost)に接続
            await _client.ConnectAsync("127.0.0.1", serverPort);
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

        DatabaseReference target = reference.Child("rooms").Child(roomId.ToString());
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
            var roomRef = reference.Child("rooms").Child(roomId);
            //切断された際の処理
            await roomRef.OnDisconnect().RemoveValue();
            //firebaseへjson形式にしたIPアドレス情報の書き込み
            await roomRef.SetRawJsonValueAsync(json);

            GameSessionData.Instance.roomId = roomId;
            //サーバの起動
            SetServer(serverPort);

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"{ex.Message}");
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
            await _udpServer.StartServer(serverPort);
        }
        else if (type == GameSessionData.ProtocolType.TCP)
        {
            //_tcpServer = new TcpGameServer(); //【TCP導入後挿入箇所】
            //await _tcpServer.StartServer(serverPort); //【TCP導入後挿入箇所】
        }
    }

    /*クライアントの選択・生成*/
    private void SetClient(int protcol)
    {
        GameSessionData.ProtocolType type = (GameSessionData.ProtocolType)protcol;
        if (type == GameSessionData.ProtocolType.UDP)
        {
            _client = new UdpGameClient();
        }
        else if (type == GameSessionData.ProtocolType.TCP)
        {
            //_client = new TcpGameClient(); //【TCP導入後挿入箇所】
        }
    }

    /*firebaseからサーバのIPアドレスの取得*/
    private async Task<DataSnapshot> GetServerIP(string roomId)
    {
        DatabaseReference target = reference.Child("rooms").Child(roomId);
        DataSnapshot snapshot = await target.GetValueAsync();

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
        //読み取ったデータを変数に代入
        string hostLocalIP = snapshot.Child("localIp").Value.ToString();
        string hostGlobalIp = snapshot.Child("globalIp").Value.ToString();

        //同じコンピュータの場合
        if(myLocalIp == hostLocalIP && myGlobalIp == hostGlobalIp)
        {
            selectServerIP = "127.0.0.1";
        }
        //ローカルネットワーク内にホストがいた場合
        else if (myGlobalIp == hostGlobalIp)
        {
            selectServerIP = hostLocalIP;
        }
        //ローカルネットワーク内にホストがいない場合
        else
        {
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

    /*プロトコルセット*/
    private void SetProtcol(int protcol)
    {
        GameSessionData.Instance.selectedProtocol = (GameSessionData.ProtocolType)protcol;
        Debug.Log($"SetProtocol:{GameSessionData.Instance.selectedProtocol}");

        //UIの表示非表示の設定
        protocolPanel.SetActive(false);
        rolePanel.SetActive(true);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(false);
    }

    /*ロールセット*/
    private void SetRole(int role)
    {
        GameSessionData.Instance.selectedRole = (GameSessionData.RoleType)role;
        Debug.Log($"SetRole:{GameSessionData.Instance.selectedRole}");
    }

    /*部屋番号セット*/
    private void SetRoomId(string roomId)
    {
        GameSessionData.Instance.roomId = roomId;
        Debug.Log($"SetRoomId:{GameSessionData.Instance.roomId}");
    }


////*UI制御用メソッド*////

    /*ボタンの有効化・無効化*/
    void SetButtonsInteractable(bool state)
    {
        if (udpButton) udpButton.interactable = state;
        if (tcpButton) tcpButton.interactable = state;
        if (hostButton) hostButton.interactable = state;
        if (clientButton) clientButton.interactable = state;
        if (roomIdDecideButton) roomIdDecideButton.interactable = state;
    }

    void SetRoomIdShow(TMP_InputField roomIdField, string roomId)
    {
        roomIdField.text = roomId;
    }

}
