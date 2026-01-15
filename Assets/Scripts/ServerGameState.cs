////*ホストのみが管理するデータ*////
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

//*サーバのみが持つ全てのクライアントの情報*//
public class ServerGameState
{
    public int maxPlayerNum = 8;//部屋の最大人数
    private int _nextPlayerId = 1;//作成するプレイヤーのID
    public Dictionary<int, MasterPlayerData> MasterPlayers = new Dictionary<int, MasterPlayerData>();


    /*プレイヤー追加メソッド*/
    public int AddPlayer(string name, System.Net.IPEndPoint ip)
    {   
        Debug.Log(maxPlayerNum);
        if(MasterPlayers.Count >= maxPlayerNum)
        {
            Debug.LogWarning("[Server] 定員オーバーのため参加を拒否しました");
            return -1;
        }

        //IDの割り当て
        int id = _nextPlayerId;
        _nextPlayerId++;//次の人のIDに更新

        //公開用プロフィールを作る
        var profile = new PlayerProfile(id,name);

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
}

//*サーバのみが持つクライアントの情報*//
public class MasterPlayerData
{
    public PlayerProfile playerProfile;
    public IPEndPoint ClientIP;
    public float LastActiveTime;
    public bool IsReady;

    public MasterPlayerData(PlayerProfile profile, System.Net.IPEndPoint ip)
    {
        playerProfile = profile;
        ClientIP = ip;
        // LastActiveTime = Time.time;
    }
}
