////*タイトル画面のUI制御スクリプト*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.Threading.Tasks;

public class TitleUIController : MonoBehaviour
{
    //UI用変数
    public GameObject playerNamePanel;
    public GameObject protocolPanel;
    public GameObject rolePanel;
    public GameObject roomConnectPanel_client;
    public GameObject selectAvatarPanel;

    public Button nameDecideButton;//名前決定時ボタン
    public Button udpButton;//UDP選択用ボタン
    public Button tcpButton;//TCP選択用ボタン
    public Button hostButton;//部屋を作成するのを選択する(ホスト)用ボタン
    public Button clientButton;//部屋に入るのを選択する(クライアント用)ボタン
    public Button roomIdDecideButton;//部屋番号を指定して決定するボタン
    public Button gameStartButton;//ゲーム開始ボタン
    public Button readyButton;//準備完了ボタン

    public Button nextAvatarButton;//次のアバターへのボタン
    public Button prevAvatarButton;//前のアバターへのボタン
    public Button[] colorButtons;//色変更のボタン

    public Button backToNamePanelButton;//名前入力画面に戻るボタン
    public Button backToProtcolPanelButton;//プロトコル選択画面に戻るボタン
    public Button backToRolePanelButton;//ロール選択画面に戻るボタン

    public TMP_InputField playerNameInput;//プレイヤー名を入力する欄
    public TMP_InputField roomIdInput;//部屋番号を入力する欄
    public TMP_InputField roomIdShow;//部屋番号を入力する欄
    public TMP_Text myPlayerNameText;//部屋にいるプレイヤーの人数を表示するテキスト
    public TMP_Text roomPlayerNumText;//部屋にいるプレイヤーの人数を表示するテキスト
    public TMP_Text[] memberNameText;//部屋にいるプレイヤーの名前を表示するテキスト
    public GameObject[] memberReadyImage;//部屋にいるプレイヤーが準備完了の場合表示する画像


    /*Start*/
    void Start()
    {
        PlayerNamePanelSet();
        //【UI】ボタンの設定
        nameDecideButton.onClick.AddListener(OnDecidePlayerName);//名前決定時ボタン
        udpButton.onClick.AddListener(OnSelectUDP);//UDP選択用ボタン
        tcpButton.onClick.AddListener(OnSelectTCP);//TCP選択用ボタン
        hostButton.onClick.AddListener(OnHostSelect);//部屋を作成するのを選択する(ホスト)用ボタン
        clientButton.onClick.AddListener(OnClientSelect);//部屋に入るのを選択する(クライアント用)ボタン
        roomIdDecideButton.onClick.AddListener(OnDecideRoomId);//部屋番号を指定して決定するボタン
        readyButton.onClick.AddListener(OnReady);//準備完了ボタン
        gameStartButton.onClick.AddListener(OnGameStart);//ゲーム開始ボタン

        backToNamePanelButton.onClick.AddListener(OnBackToNamePanel);//名前入力画面に戻るボタン
        backToProtcolPanelButton.onClick.AddListener(OnBackToProtcolPanel);//プロトコル選択画面に戻るボタン
        backToRolePanelButton.onClick.AddListener(OnBackToRolePanel);//ロール選択画面に戻るボタン

        //NetworkManagerのイベントとメソッドを紐づける
        NetworkManager.Instance.OnJoinRoomSuccess += OnJoinRoomSuccess;//部屋参加成功時
        NetworkManager.Instance.OnJoinRoomFailed += OnJoinRoomFailed;//部屋参加失敗時
        NetworkManager.Instance.OnMemberListUpdate += OnMemberListUpdate;//参加者の更新時

        NetworkManager.Instance.OnHostAllReady += UpdateStartButtonState;//参加者の更新時
        

        for (int i = 0; i < GameSessionData.Instance.playerLimitNum; i++)
        {
            memberReadyImage[i].SetActive(false);
            memberNameText[i].gameObject.SetActive(false);
        }
    }

    /*Destory オブジェクト破壊時*/
    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            //イベント解除
            NetworkManager.Instance.OnJoinRoomSuccess -= OnJoinRoomSuccess;
            NetworkManager.Instance.OnJoinRoomFailed -= OnJoinRoomFailed;
            NetworkManager.Instance.OnMemberListUpdate -= OnMemberListUpdate;
            NetworkManager.Instance.OnHostAllReady -= UpdateStartButtonState;
        }
    }

    /*プレイヤー名決定*/
    void OnDecidePlayerName()
    {
        SetButtonsInteractable(false);
        GameSessionData.Instance.SetPlayerName(playerNameInput.text);//GameSessionDataに入力した名前を登録
        myPlayerNameText.text = GameSessionData.Instance.myName;//アバター選択画面の自分の名前を表示
        ProtcolSelectPanelSet();
        SetButtonsInteractable(true);
    }

    /*プロトコル選択-UDP選択*/
    void OnSelectUDP()
    {
        SetButtonsInteractable(false);
        NetworkManager.Instance.OnSelectUDP();
        RoleSelectPanelSet();
        SetButtonsInteractable(true);
    }

    /*プロトコル選択-TCP選択*/
    void OnSelectTCP()
    {
        SetButtonsInteractable(false);
        NetworkManager.Instance.OnSelectTCP();
        RoleSelectPanelSet();
        SetButtonsInteractable(true);
    }

    /*ロール選択-ホスト選択*/
    async void OnHostSelect()
    {
        SetButtonsInteractable(false);
        string roomId = await NetworkManager.Instance.OnHostSelect();
        if (roomId == null)
        {
            Debug.Log("部屋作成失敗");
            SetButtonsInteractable(true);
            return;
        }
        AvatarSelectPanelSet();
        SetRoomIdShow(roomIdShow, roomId);
        SetButtonsInteractable(true);
    }

    /*ロール選択-クライアント選択*/
    void OnClientSelect()
    {
        SetButtonsInteractable(false);
        NetworkManager.Instance.OnClientSelect();
        RoomConnectPanelSet();
        SetButtonsInteractable(true);
    }

    /*部屋番号入力決定(部屋接続)*/
    async void OnDecideRoomId()
    {
        SetButtonsInteractable(false);
        string roomId = await NetworkManager.Instance.OnDecideRoomId(roomIdInput.text);
        if (roomId == null)
        {
            roomIdInput.text = "";

            // UIを元に戻す（部屋番号入力画面のまま）
            RoomConnectPanelSet();

            //【UI】ボタンを再度押せるように戻す
            roomIdDecideButton.interactable = true;

            //【UI】カーソルを入力欄に戻してあげる（PC操作の場合便利）
            await Task.Delay(10);
            roomIdInput.ActivateInputField();
            roomIdInput.Select();

            SetButtonsInteractable(true);
            return;
        }
    }

    /*準備完了が押されたとき*/
    public void OnReady()
    {
        if (!GameSessionData.Instance.isReady)
        {
            GameSessionData.Instance.isReady = true;//自分のデータを準備完了にする
            readyButton.interactable = false;//準備完了ボタンを無効化する
        }
        SelectAvatarInteractable(false);//アバター変更ボタンの無効化
        NetworkManager.Instance.SendAvatarDataToHost();//ホストに決定したアバターデータを送る
    }

    /*ゲーム開始が押された時*/
    public void OnGameStart()
    {
        NetworkManager.Instance.OnGameStart();
    }

    /*名前入力に戻るボタンが押された時*/
    public void OnBackToNamePanel()
    {
        PlayerNamePanelSet();
    }

    /*プロトコル選択に戻るボタンが押された時*/
    public void OnBackToProtcolPanel()
    {
        ProtcolSelectPanelSet();
    }

    /*ロール選択に戻るボタンが押された時*/
    public void OnBackToRolePanel()
    {
        RoleSelectPanelSet();
        NetworkManager.Instance.DisconnectLocal();
    }

    //*イベントを受け取った際に呼び出す(Udp/TcpNetwork → NetworkManager → TitleUIController)*//
    /*部屋接続成功時*/
    void OnJoinRoomSuccess()
    {
        SetRoomIdShow(roomIdShow, GameSessionData.Instance.roomId);
        AvatarSelectPanelSet();
        SetButtonsInteractable(true);
    }

    /*部屋接続失敗時*/
    void OnJoinRoomFailed(string reason)
    {
        Debug.Log($"接続失敗: {reason}");

        // 入力欄をクリア
        roomIdInput.text = "";

        // UIを元に戻す（部屋番号入力画面のまま）
        RoomConnectPanelSet();

        //【UI】ボタンを再度押せるように戻す
        roomIdDecideButton.interactable = true;

        //【UI】カーソルを入力欄に戻してあげる（PC操作の場合便利）
        roomIdInput.ActivateInputField();
        roomIdInput.Select();

        SetButtonsInteractable(true);
    }

    /*参加プレイヤーの更新*/
    void OnMemberListUpdate()
    {
        int i = 0;
        //UIのプレイヤー名一覧をデータの全プレイヤーデータの名前に更新
        foreach (var player in GameSessionData.Instance.AllPlayers)
        {
            memberNameText[i].gameObject.SetActive(true);
            memberNameText[i].text = player.Value.name;
            //準備完了な場合
            if (player.Value.isReady)
            {
                memberReadyImage[i].SetActive(true);
                Debug.Log($"{i}番目のユーザ準備完了");
            }
            else
            {
                memberReadyImage[i].SetActive(false);
                Debug.Log($"{i}番目のユーザ準備未完了");
            }
            i += 1;
        }
        //部屋の参加人数の更新
        roomPlayerNumText.text = $"{GameSessionData.Instance.AllPlayers.Count.ToString()}/{GameSessionData.Instance.playerLimitNum}";
        i = 0;
    }

    /*ゲーム開始ボタンの有効化*/
    void UpdateStartButtonState(bool isAllReady)
    {
        if(gameStartButton != null)
        {
            GameStartButtonInteractable(isAllReady);
            Debug.Log($"[UI]全員準備完了{isAllReady}");
        }
    }


    ////*UI制御用メソッド*//////【UI】

    /*ボタンの有効化・無効化*/
    void SetButtonsInteractable(bool state)
    {
        if (udpButton) udpButton.interactable = state;
        if (tcpButton) tcpButton.interactable = state;
        if (hostButton) hostButton.interactable = state;
        if (clientButton) clientButton.interactable = state;
        if (roomIdDecideButton) roomIdDecideButton.interactable = state;
    }

    /*部屋番号の表示*/
    void SetRoomIdShow(TMP_InputField roomIdField, string roomId)
    {
        roomIdField.text = roomId;
    }

    /*プレイヤー名入力のUIパネルをオンにする*/
    void PlayerNamePanelSet()
    {
        playerNamePanel.SetActive(true);
        protocolPanel.SetActive(false);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(false);
    }

    /*プロトコル選択画面のUIパネルをオンにする*/
    void ProtcolSelectPanelSet()
    {
        playerNamePanel.SetActive(false);
        protocolPanel.SetActive(true);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(false);
    }

    /*ロール選択画面のUIパネルをオンにする*/
    void RoleSelectPanelSet()
    {
        playerNamePanel.SetActive(false);
        protocolPanel.SetActive(false);
        rolePanel.SetActive(true);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(false);
    }

    /*部屋番号入力(部屋接続)のUIパネルをオンにする*/
    void RoomConnectPanelSet()
    {
        playerNamePanel.SetActive(false);
        protocolPanel.SetActive(false);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(true);
        selectAvatarPanel.SetActive(false);
    }

    /*アバター選択画面のUIパネルをオンにする*/
    void AvatarSelectPanelSet()
    {
        playerNamePanel.SetActive(false);
        protocolPanel.SetActive(false);
        rolePanel.SetActive(false);
        roomConnectPanel_client.SetActive(false);
        selectAvatarPanel.SetActive(true);

        //ゲーム開始ボタンの有効・無効
        //ホストならゲーム開始ボタンを表示
        if (GameSessionData.Instance.selectedRole == GameSessionData.RoleType.Host)
        {
            gameStartButton.gameObject.SetActive(true);
            gameStartButton.interactable = false;
        }
        //クライアントならゲーム開始ボタンを非表示に
        else
        {
            gameStartButton.gameObject.SetActive(false);
        }
    }

    /*アバター変更ボタンの有効化・無効化*/
    void SelectAvatarInteractable(bool state)
    {
        nextAvatarButton.interactable = state;
        prevAvatarButton.interactable = state;
        foreach(Button colorButton in colorButtons)
        {
            colorButton.interactable = state;
        }
    }

    /*ゲーム開始ボタンの有効化・無効化*/
    void GameStartButtonInteractable(bool status)
    {
        gameStartButton.interactable = status;
    }
}
