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
    //クライアント
    public IGameClient _client;
    //サーバ
    public UdpGameServer _udpServer;
    //public TcpGameServer _tcpServer;【TCP導入後挿入箇所】

    /*自分自身のデータ*/
    public int myIDNum;
    public int myAvatarIndex;
    public int myColorIndex;
    public string myName;

    /*人数*/
    public int playerLimitNum = 8;//プレイヤー数の制限
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

        //テスト用
        var player1 = new PlayerProfile(1,"haruru",1,3,new Vector3(10,0,10),false);
        var player2 = new PlayerProfile(2,"natu",2,5,new Vector3(20,0,20),true);
        AllPlayers.Add(player1.id,player1);
        Debug.Log($"player1 colorIdx/{AllPlayers[player1.id].colorIdx}");
        AllPlayers.Add(player2.id,player2);
        Debug.Log($"player2 colorIdx/{AllPlayers[player2.id].colorIdx}");
        
    }


    public void SetMyAvatar(int avatarIdx, int colorIdx)
    {
        myAvatarIndex = avatarIdx;
        myColorIndex = colorIdx;

        //テスト用
        myIDNum = 0;
        var myPlayer = new PlayerProfile(myIDNum,myName,myAvatarIndex,myColorIndex,new Vector3(0,0,0),false);
        AllPlayers.Add(myIDNum,myPlayer);
        Debug.Log($"mycolorIdx/{AllPlayers[myIDNum].colorIdx}");
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
