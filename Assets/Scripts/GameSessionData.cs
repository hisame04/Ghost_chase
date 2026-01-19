////*全クライアントが持つ1ゲームのデータ*////
using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

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
    public string serverIP;
    public string serverPort;
    public int clientPort;

    /*自分自身のデータ*/
    public int myIDNum;
    public int myAvatarIndex;
    public int myColorIndex;
    public string myName;
    public bool isReady;

    /*人数*/
    public int playerLimitNum = 1;//プレイヤー数の制限
    public int playerNum = 3;//実際のプレイヤー数

    /*初期リスポーン時のデータ*/
    public Vector3[] spawnPos;//接続人数が確定次第PlayerProfileに流し込む
    public int[] avatarIdx;//ホストが全員から集め次第配ってPlayerProfileに流し込む

    /*全プレイヤーのデータ*/
    public Dictionary<int, PlayerProfile> AllPlayers = new Dictionary<int, PlayerProfile>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても消えないようにする
        }
        else
        {
            Destroy(gameObject); // 重複して存在しないようにする
        }
        
    }


    public void SetMyAvatar(int avatarIdx, int colorIdx)
    {
        myAvatarIndex = avatarIdx;
        myColorIndex = colorIdx;
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

    //id指定でプレイヤーの情報(PlayerProfile)を探す
    public PlayerProfile GetPlayer(int id)
    {
        if (AllPlayers.ContainsKey(id))
        {
            return AllPlayers[id];
        }
        return null;
    }
}
