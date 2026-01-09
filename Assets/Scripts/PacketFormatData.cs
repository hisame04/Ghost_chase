using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 通信メッセージの種類
public enum NetworkCommand
{
    ConnectRequest,    // 接続要求
    ConnectResponse, //接続応答
    SendAvatarData, //プレイヤー基本情報(アバターなど)
    AllPlayerListUpdate, //部屋に参加しているプレイヤー情報
    GameStart,  // ゲーム開始
    // Move,       // 位置情報
    //Action //タッチ
    //WorldStateUpdate //プレイヤーの位置と状態更新
    // ... 今後追加
}

[System.Serializable]
/*パケットの基本形*/
public class GamePacket
{
    public NetworkCommand command; // 何の用事か
    public string dataJson;        // 中身（さらに別のクラスをJSONにしたもの）
}

[System.Serializable]
/*接続時に送るデータの中身*/
public class ConnectRequestData
{
    public string playerName; // プレイヤー名
    // 将来ここにアバターIDなどを追加
}

/*接続応答として送るデータの中身*/
public class ConnectResponseData
{
    public int playerId; //プレイヤーに割り当てたID番号
}

/*ホストに送るアバターデータの中身*/
public class SendAvatarData
{
    
}

[System.Serializable]
/*ホストがfirebaseにIPアドレスのデータを登録するためのクラス*/
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
