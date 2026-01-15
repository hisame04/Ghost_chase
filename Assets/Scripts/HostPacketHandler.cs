////*ホストの受信を処理するためのスクリプト*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Xml.Serialization;

public class HostPacketHandler
{
    private NetworkManager _manager;
    // Start is called before the first frame update
    public HostPacketHandler(NetworkManager manager)
    {
        _manager = manager;
    }

    public void Handle(string json, IPEndPoint sender)
    {
        // JSONをパケットの形に戻す
        GamePacket packet = JsonUtility.FromJson<GamePacket>(json);

        Debug.Log($"[HostHandler] コマンド受信: {packet.command}");

        switch (packet.command)
        {
            case NetworkCommand.ConnectRequest:
                // クライアント接続処理
                HandleConnectRequest(packet.dataJson, sender);
                break;
            case NetworkCommand.SendAvatarData:
                // クライアントのアバターデータ受信処理
                break;
        }
    }

    private void HandleConnectRequest(string dataJson, IPEndPoint sender)
    {
        //受け取ったデータを接続要求のクラスに戻す
        ConnectRequestData data = JsonUtility.FromJson<ConnectRequestData>(dataJson);

        //ホストの持つ全プレイヤーデータに接続したプレイヤーの情報を追加 //発行されたIDが戻ってくる
        int resId = _manager.ServerState.AddPlayer(data.playerName, sender);

        //人数が満員かどうかチェック
        if(resId == -1)
        {
            Debug.Log($"拒否: {sender} からの接続（満員）");
            var connectRefusedData = new ConnectRefusedData {reason = "FULL"};
            _manager.SendPacketToClient(NetworkCommand.ConnectRefused, connectRefusedData, sender);
            return;
        }

        Debug.Log($"接続要求：{_manager.ServerState.MasterPlayers[resId].playerProfile.name}");

        //接続要求に対する返答の送信
        var connectResponseData = new ConnectResponseData {playerId = resId};
        _manager.SendPacketToClient(NetworkCommand.ConnectResponse, connectResponseData, sender);
    }

}
