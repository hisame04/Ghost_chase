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
            case NetworkCommand.ConnectAnounce:
                //接続拒否したときの処理
                HandleConnectAnounce(packet.dataJson);
                break;
            case NetworkCommand.GameStart:
                //ゲーム開始時の処理
                break;
        }
    }


    //*コマンドごとの処理のメソッド*//
    /*接続応答を受け取った時の処理*/
    private void HandleConnectResponse(string dataJson)
    {
        ConnectResponseData data = JsonUtility.FromJson<ConnectResponseData>(dataJson);
        Debug.Log($"部屋参加許可 ID：{data.playerId}");

        _manager.TriggerJoinRoomSuccess();

        //接続した部屋番号の保存
        GameSessionData.Instance.roomId = data.roomId;

        // //プレイヤーリストの初期化
        foreach (var player in data.firstPlayerList)
        {
            GameSessionData.Instance.AllPlayers.Add(player.id, new PlayerProfile(player.id, player.name, player.isReady));
        }
        Debug.Log($"[client]playerNum{GameSessionData.Instance.AllPlayers.Count}");

        _manager.TriggerMemberListUpdate();
    }

    /*接続拒否を受け取った時の処理*/
    private void HandleConnectRefused(string dataJson)
    {
        ConnectRefusedData data = JsonUtility.FromJson<ConnectRefusedData>(dataJson);
        Debug.Log($"部屋参加拒否 理由：{data.reason}");

        _manager.TriggerJoinRoomFailed(data.reason);
    }

    /*新しいプレイヤーの接続通知を受け取った時の処理*/
    private void HandleConnectAnounce(string dataJson)
    {
        LobbyPlayerData data = JsonUtility.FromJson<LobbyPlayerData>(dataJson);
        Debug.Log($"接続通知：{data.id}");

        if (!GameSessionData.Instance.AllPlayers.ContainsKey(data.id))
        {
            GameSessionData.Instance.AllPlayers.Add(data.id, new PlayerProfile(data.id, data.name, data.isReady));
            _manager.TriggerMemberListUpdate();
        }
    }
}
