////*クライアントの受信を処理するためのスクリプト*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPacketHandler
{
    private NetworkManager _manager;//NetworManager

    public ClientPacketHandler(NetworkManager manager)
    {
        _manager = manager;
    }

    // クライアントがデータを受け取ったときの処理
    public void Handle(string json)
    {
        GamePacket packet = JsonUtility.FromJson<GamePacket>(json);

        Debug.Log($"[ClientHandler] コマンド受信: {packet.command}");

        switch (packet.command)
        {
            case NetworkCommand.ConnectResponse:
                // 接続成功したときの処理
                HandleConnectResponse(packet.dataJson);
                break;
            case NetworkCommand.ConnectRefused:
                //接続拒否したときの処理
                HandleConnectRefused(packet.dataJson);
                break;
            case NetworkCommand.GameStart:
                //ゲーム開始時の処理
                break;
        }
    }

    private void HandleConnectResponse(string dataJson)
    {
        ConnectResponseData data = JsonUtility.FromJson<ConnectResponseData>(dataJson);
        Debug.Log($"部屋参加許可 ID：{data.playerId}");

        _manager.TriggerJoinRoomSuccess();
    }


    private void HandleConnectRefused(string dataJson)
    {
        ConnectRefusedData data = JsonUtility.FromJson<ConnectRefusedData>(dataJson);
        Debug.Log($"部屋参加拒否 理由：{data.reason}");

        _manager.TriggerJoinRoomFailed(data.reason);
    }
}
