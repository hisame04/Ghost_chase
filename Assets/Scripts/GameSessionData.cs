////*全クライアントが持つ1ゲームのデータ*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; private set; }

    /*通信の設定*/
    //通信方法の選択肢
    public enum ProtocolType { UDP, TCP }
    public enum RoleType { Host, Client }
    //実際の値
    public ProtocolType selectedProtocol;
    public RoleType selectedRole;
    //IPアドレス情報
    public string roomId;

    /*自分自身のデータ*/
    public int myIDNum;
    public int myAvatarIndex;
    public int myColorIndex;
    public string myName;
    public bool isReady;

    /*人数*/
    public int playerLimitNum = 6;//プレイヤー数の制限
    public int playerNum = 3;//実際のプレイヤー数

    /*制限時間*/    
    public float gameTimeLimit;
    public float waitStartTime;

    /*全プレイヤーのデータ*/
    public Dictionary<int, PlayerProfile> AllPlayers = new Dictionary<int, PlayerProfile>();

    /*RuleDataのスクリプタブルオブジェクト*/
    [SerializeField] private RuleData ruleData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても消えないようにする
            ApplyGameRule(ruleData);//ルールのスクリプタブルオブジェクトから値をコピー
        }
        else
        {
            Destroy(gameObject); // 重複して存在しないようにする
        }
        
    }

    /*自分のデータを初期化*/
    public void ResetGameSessionMyData(int id)
    {
        SetPlayerIdNum(id);
        SetMyAvatarIdx(0);
        SetMyColorIdx(0);
        isReady=false;
    }

    /*自分のアバター番号をセットする*/
    public void SetMyAvatarIdx(int avatarIdx)
    {
        myAvatarIndex = avatarIdx;
    }

    /*自分の色番号をセットする*/
    public void SetMyColorIdx(int colorIdx)
    {
        myColorIndex = colorIdx;
    }

    /*プレイヤーIDをセットする*/
    public void SetPlayerIdNum(int id)
    {
        myIDNum = id;
    }

    /*プレイヤー名をセットする*/
    public void SetPlayerName(string playerName)
    {
        myName = playerName;
    }

    /*プロトコルセット*/
    public void SetProtcol(int protcol)
    {
        selectedProtocol = (GameSessionData.ProtocolType)protcol;
        Debug.Log($"SetProtocol:{GameSessionData.Instance.selectedProtocol}");
    }

    /*ロールセット*/
    public void SetRole(int role)
    {
        selectedRole = (GameSessionData.RoleType)role;
        Debug.Log($"SetRole:{GameSessionData.Instance.selectedRole}");
    }

    /*部屋番号セット*/
    public void SetRoomId(string selectRoomId)
    {
        roomId = selectRoomId;
        Debug.Log($"SetRoomId:{GameSessionData.Instance.roomId}");
    }

    /*id指定でプレイヤーの情報(PlayerProfile)を探す*/
    public PlayerProfile GetPlayer(int id)
    {
        if (AllPlayers.ContainsKey(id))
        {
            return AllPlayers[id];
        }
        return null;
    }

    /*RuleDataからルールの読み込み*/
    public void ApplyGameRule(RuleData rule)
    {
        if(rule == null) return;

        playerLimitNum = rule.max_players;
        gameTimeLimit = rule.limitTime;
        waitStartTime = rule.waitStartTime;
        
        Debug.Log($"ルール適用: {rule.max_players}, 最大人数: {playerLimitNum}");
        Debug.Log($"ルール適用: {rule.limitTime}, 制限時間: {gameTimeLimit}");
        Debug.Log($"ルール適用: {rule.waitStartTime}, 制限時間: {waitStartTime}");
    }

    /*プレイヤーデータの削除*/
    public void RemovePlayer(int id)
    {
        var player = GetPlayer(id);
        if(player != null)
        {
            if(player.playerObject != null)
            {
                //ゲーム画面のオブジェクトの削除
                GameObject.Destroy(player.playerObject);
                Debug.Log($"[GameSessionData]{id}番のオブジェクトを削除しました");
            }
            if(player.playerObject == null)
            {
                Debug.Log($"[GameSessionData]{id}番のオブジェクトがnullです");
            }

            AllPlayers.Remove(id);
            Debug.Log($"[GameSessionData] {id}番のデータを削除しました");
        }
        else
        {
            Debug.Log($"[GameSessionData] {id}番のデータがありません");
        }

        if(AllPlayers.Count < 2)//プレイヤーが1人になったらタイトルに戻す
        {
            if(GameManager.Instance != null && GameManager.Instance.onGame)//ゲーム中の場合のみ
            {
                NetworkManager.Instance.DisconnectLocal();
                SceneManager.LoadScene("TitleScene");
                //マウスカーソルを表示
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

            }
        }
    }

    /*初期化用メソッド*/
    public void ResetAllState()
    {
        roomId = "";
        AllPlayers.Clear();
        myIDNum = 0;
        isReady = false;
        myAvatarIndex = 0;
        myColorIndex=0;

        if(ruleData != null)
        {
            ApplyGameRule(ruleData);
        }

        Debug.Log("[GameSessionData] ステータスをリセットしました");
    }

}
