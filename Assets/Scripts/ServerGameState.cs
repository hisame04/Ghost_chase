////*ホストのみが管理するデータ*////
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Linq;
using System.Data.Common;

//*サーバのみが持つ全てのクライアントの情報*//
public class ServerGameState
{
    private readonly List<Vector3> _spawnPos = new List<Vector3>
    {
        new Vector3(0, 1, 0),
        new Vector3(10, 1, 0),
        new Vector3(-10, 1, 0),
        new Vector3(0, 1, 10),
        new Vector3(0, 1, -10),
        new Vector3(5, 1, 5)
    };

    public int maxPlayerNum = 6;//部屋の最大人数
    public int currentPlayerNum = 0;
    public bool allReady = false;
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

        //現在の人数を更新
        currentPlayerNum+=1;

        //人数が正しいかチェック
        CheckPlayerNum();

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

    /*回転の更新メソッド*/
    public void UpdateRotation(int playerId, Quaternion rot)
    {
        if (MasterPlayers.ContainsKey(playerId))
        {
            MasterPlayers[playerId].playerProfile.currentRot = rot;
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

    /*人数チェック*/
    public void CheckPlayerNum()
    {
        //人数とデータがずれていないか確認
        if(currentPlayerNum!=MasterPlayers.Count){
            currentPlayerNum = MasterPlayers.Count;
            Debug.Log("[Host] マスターデータと人数がずれていました");
        }
    }

    /*準備完了人数チェック*/
    public bool CheckReadyNum()
    {
        foreach (var player in MasterPlayers)
        {
            if (!player.Value.isReady)
            {
                return false;//準備完了ではない人がいたらfalse
            }
        }

        if(currentPlayerNum != GameSessionData.Instance.playerLimitNum)
        {
            return false;//人数がそもそも足りていなければfalse
        }
        
        return true;
    }

    //準備完了のデータのセット
    public void SetReady(int id, bool isReady)
    {
        MasterPlayers[id].isReady = isReady;
        GetPlayer(id).isReady=isReady;
    }

    //アバターデータのセット
    public void SetAvatarData(int id, int avatarIdx, int colorIdx)
    {
        var player =GetPlayer(id);
        player.avatarIdx = avatarIdx;
        player.colorIdx = colorIdx;
    }

    /*ゲーム開始時に鬼と初期位置を決める処理*/
    public void DecideInitialGameData()
    {
        List<int> playerIds = MasterPlayers.Keys.ToList();
        int playerCount = playerIds.Count;

        if (playerCount == 0) return;

        //_spwnPosをランダムに並び替えした座標リストを作成
        List<Vector3> shuffledSpawns = _spawnPos.OrderBy(x => System.Guid.NewGuid()).ToList();

        //鬼を決定
        int tagIdx = Random.Range(0,playerCount);
        int tagPlayerId = playerIds[tagIdx];

        for(int i = 0;i<playerCount; i++)
        {
            int id = playerIds[i];
            var masterData = MasterPlayers[id];

            if (i < shuffledSpawns.Count)//インデックスが初期位置の数に収まった場合
            {
                masterData.playerProfile.firstSpawnPos = shuffledSpawns[i];
            }
            else
            {
                Debug.Log($"[ServerState]初期位置がたりません 初期位置{shuffledSpawns.Count}、プレイヤー数{playerCount}");
                masterData.playerProfile.firstSpawnPos = Vector3.zero;
            }

            if(id == tagPlayerId)
            {
                masterData.playerProfile.isTag = true;
                Debug.Log($"[ServerState]最初の鬼はid{id}の人");
            }
            else
            {
                masterData.playerProfile.isTag = false;
            }
        }
    }

    /*id指定で鬼かどうかのフラッグを切り替える*/
    public void UpdatePlayerIsTag(int id,bool isTag)
    {
        if(GetPlayer(id) != null){
            GetPlayer(id).isTag = isTag;
        }
    }

    /*鬼が何番かをチェック*/
    public int CheckTagPlayer()
    {
        int i = 0;
        int tagId = 0;
        foreach(var player in MasterPlayers)
        {
            if (player.Value.playerProfile.isTag)
            {
                if(player.Key == player.Value.playerProfile.id)
                {
                    tagId = player.Key;
                    i += 1;
                }
                else
                {
                    Debug.Log("[Host]マスターデータのIDが一致しません");
                    return -1;
                }
            }
        }
        if (i < 2)
        {
            return tagId;
        }
        else
        {
            return -1;
        }
    }

    /*鬼の判定の修正*/
    public void FixTagData()
    {
        
    }

    /*ロードシーン完了フラグのセット*/
    public void SetLoadSuccess(int id , bool load)
    {
        if(MasterPlayers.ContainsKey(id)){
            MasterPlayers[id].isSuccessLoaded = load;
        }
    }

    /*全員完了したかのチェク*/
    public bool CheckAlllLoadSuccess()
    {
        foreach (var player in MasterPlayers)
        {
            if (!player.Value.isSuccessLoaded)
            {
                return false;//準備完了ではない人がいたらfalse
            }
        }

        if(currentPlayerNum != GameSessionData.Instance.playerLimitNum)
        {
            return false;//人数がそもそも足りていなければfalse
        }
        
        return true;
    }

    public void ResetAllStates()
    {
        MasterPlayers.Clear();
        allReady = false;
        currentPlayerNum = 0;
        _nextPlayerId = 1;
        Debug.Log("[ServerGameState] ステータスをリセットしました");
    }
}

//*サーバのみが持つクライアントの情報*//
public class MasterPlayerData
{
    public PlayerProfile playerProfile;
    public IPEndPoint clientIP;
    public float lastActiveTime;
    public bool isReady;//ロビーでの準備完了フラグ
    public bool isSuccessLoaded;//ゲームシーン遷移完了フラグ

    public MasterPlayerData(PlayerProfile profile, System.Net.IPEndPoint ip)
    {
        isReady = false;
        isSuccessLoaded = false;
        playerProfile = profile;
        clientIP = ip;
        // LastActiveTime = Time.time;
    }
}
