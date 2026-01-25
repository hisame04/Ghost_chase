////*通信のパケット形式を定めたデータ*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 通信メッセージの種類
public enum NetworkCommand
{
    ConnectRequest,//【from Client】接続要求
    ConnectResponse,//【from Host】接続応答
    ConnectRefused,//【from Host】接続失敗
    ConnectAnnounce,//【from Host】接続通知
    Disconnect,//【from Host】【from Client】切断通知
    SendAvatarData,//【from Client】プレイヤー基本情報(アバターなど)
    ReadyAnnounce,//【from Host】部屋に参加しているプレイヤーが準備完了の場合
    GameStart,//【from Host】ゲーム開始(全プレイヤーの初期データを送る)
    Move,       //【from Client】 位置情報
    Touch, //【from Client】タッチ
    TagChange, //【from Host】鬼の交代情報
    UpdatePos, //【from Host】プレイヤーの位置と状態更新
    LoadSuccess,//【from Client】ゲーム画面遷移完了
    StartCountDown, //【from Host】カウントダウン開始
    TimeRemain,//【from Host】制限時間同期
}

/*パケットの基本形*/
[System.Serializable]
public class GamePacket
{
    public NetworkCommand command; // 何の用事か
    public string dataJson;        // 中身（さらに別のクラスをJSONにしたもの）
}

//*【ConnectRequest】*//
/*接続時に送るデータの中身*/
[System.Serializable]
public class ConnectRequestData
{
    public string playerName; // プレイヤー名
    // 将来ここにアバターIDなどを追加
}

//*【ConnectResponse】*//
/*接続成功として送るデータの中身*/
[System.Serializable]
public class ConnectResponseData
{
    public string roomId; //部屋番号
    public int playerId; //プレイヤーに割り当てたID番号
    public List<LobbyPlayerData> firstPlayerList; //部屋に参加しているプレイヤーのリスト
}

//*【ConnectRefused】*//
/*接続失敗として送るデータの中身*/
[System.Serializable]
public class ConnectRefusedData
{
    public string reason; //接続できない理由
}

//*【Disconnect】*//
/*切断時に送るデータの中身*/
[System.Serializable]
public class DisconnectData
{
    public int playerId;
}

//*【SendAvatarData】*//
/*ホストに送るアバターデータの中身*/
[System.Serializable]
public class SendAvatarData
{
    public int id;
    public bool isReady;
    public int avatarIdx;
    public int colorIdx;
    public SendAvatarData(int id,bool isReady,int avatarIdx,int colorIdx)
    {
        this.id = id;
        this.isReady = isReady;
        this.avatarIdx = avatarIdx;
        this.colorIdx = colorIdx;
    }
}

//*【GameStart】*//
/*ゲーム開始時にホストが全プレイヤーに配るデータ*/
[System.Serializable]
public class GameStartPlayerDataList
{
    public List<GameStartPlayerData> startPlayerList;
    public string stage;
    public GameStartPlayerDataList(List<GameStartPlayerData> startPlayerList,string stage)
    {
        this.startPlayerList = startPlayerList;
        this.stage = stage;
    }
}

//ゲーム開始時のプレイヤー(1人)の情報
[System.Serializable]
public class GameStartPlayerData
{
    public int id;
    public int avatarIdx;
    public int colorIdx;
    public Vector3 startPos;
    public bool isTag;
    
    public GameStartPlayerData(int id, int avatarIdx,int colorIdx, Vector3 startPos,bool isTag){
        this.id = id;
        this.avatarIdx = avatarIdx;
        this.colorIdx = colorIdx;
        this.startPos = startPos;
        this.isTag = isTag;
    }
}

//*【ReadyAnnounce】*//
/*ホストがロビーでクライアントに送る1人分のプレイヤーデータ*/
[System.Serializable]
public class LobbyPlayerData
{
    public int id;
    public string name;
    public bool isReady;
    public LobbyPlayerData(int id,string name,bool isReady)
    {
        this.id = id;
        this.name = name;
        this.isReady = isReady;
    }
}

//*【Touch】*//
/*タッチした時鬼のクライアントがホストに送るデータ*/
[System.Serializable]
public class TouchReportData
{
    public int playerId;
    public int collisionId;
    
}

//*【TagChange】*//
/*ホストが鬼交代の時に全員に送るデータ*/
[System.Serializable]
public class TagChangeData
{
    public List<TagStateData> playerTagList;
    public TagChangeData(List<TagStateData> playerTagList)
    {
        this.playerTagList = playerTagList;
    }
}

/*ホストが鬼交代の時に送る鬼かどうかの一人分のデータ*/
[System.Serializable]
public class TagStateData
{
    public int id;
    public bool isTag;
    public TagStateData(int id, bool isTag)
    {
        this.id = id;
        this.isTag = isTag;
    }
}

/*ホストがfirebaseにIPアドレスのデータを登録するためのクラス*/
[System.Serializable]
public class RoomData
{
    public string localIp;
    public string globalIp;
    public int port;

    public RoomData(string localIp, string globalIp, int port)
    {
        this.localIp = localIp;
        this.globalIp = globalIp;
        this.port = port;
    }
}

//*【Move】*//
[System.Serializable]
public class MoveStateData
{
    public int id;
    public Vector3 pos;//位置
    public Quaternion rot;//回転
}

//*【UpdatePos】*//
/*全プレイヤー*/
[System.Serializable]
public class MoveAnnounceData
{
    public List<MoveStateData> moveStateList;
}

//*【LoadSccess】*//
/*シーン遷移成功時*/ //通信コストを減らすなら空のデータにしてもok
[System.Serializable]
public class LoadSuccessData
{
    public int id;
    public string loadScene;
}

//*【StartCountDown】*//
/*カウントダウン開始通知*/ //通信コストを減らすなら空のデータにしてもok
[System.Serializable]
public class StartCounDownData
{
    public float countTime;
}

//*【TimeRemain】*//
/*時間同期*/
[System.Serializable]
public class TimeRemainData
{
    public float currentTime;
}