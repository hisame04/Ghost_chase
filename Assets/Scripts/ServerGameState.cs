////*ホストのみが管理するデータ*////
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

//*サーバのみが持つ全てのクライアントの情報*//
public class ServerGameState
{
    public int maxPlayerNum = 8;//部屋の最大人数
    public int currentPlayerNum = 0;
    private int _nextPlayerId = 1;//作成するプレイヤーのID
    public Dictionary<int, MasterPlayerData> MasterPlayers = new Dictionary<int, MasterPlayerData>();


    /*プレイヤー追加メソッド*/
    public int AddPlayer(string name, System.Net.IPEndPoint ip)
    {   
        if(MasterPlayers.Count >= maxPlayerNum)
        {
            Debug.LogWarning("[Server] 定員オーバーのため参加を拒否しました");
            return -1;
        }

        //IDの割り当て
        int id = _nextPlayerId;
        _nextPlayerId++;//次の人のIDに更新

        //公開用プロフィールを作る
        var profile = new PlayerProfile(id,name,false);

        //管理用データを作る（プロフィール + IP）
        var masterData = new MasterPlayerData(profile,ip);

        //辞書に登録
        MasterPlayers.Add(id, masterData);

        //発行したIDを返す
        return id;
    }

    /*位置の更新メソッド*/
    public void UpdatePosition(int playerId, Vector3 pos)
    {
        if (MasterPlayers.ContainsKey(playerId))
        {
            MasterPlayers[playerId].playerProfile.currentPos = pos;
        }
    }

    /*id指定でプレイヤーの情報(PlayerProfile)を探す*/
    public PlayerProfile GetPlayer(int id)
    {
        if (MasterPlayers.ContainsKey(id))
        {
            Debug.Log($"[ServerGameState]{id}のプレイヤーを取得");
            return MasterPlayers[id].playerProfile;
        }
        Debug.Log($"[ServerGameState]{id}のプレイヤーは存在しません");
        return null;
    }
}

//*サーバのみが持つクライアントの情報*//
public class MasterPlayerData
{
    public PlayerProfile playerProfile;
    public IPEndPoint clientIP;
    public float lastActiveTime;
    public bool isReady;

    public MasterPlayerData(PlayerProfile profile, System.Net.IPEndPoint ip)
    {
        playerProfile = profile;
        clientIP = ip;
        // LastActiveTime = Time.time;
    }
}
