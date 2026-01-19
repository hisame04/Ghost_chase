////*ホストの受信を処理するためのスクリプト*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Xml.Serialization;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

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
        //接続不可だった場合
        if(resId == -1)
        {
            Debug.Log($"拒否: {sender} からの接続（満員）");
            var connectRefusedData = new ConnectRefusedData {reason = "FULL"};
            _manager.SendPacketToClient(NetworkCommand.ConnectRefused, connectRefusedData, sender);
            return;
        }

        Debug.Log($"接続要求：{_manager.ServerState.MasterPlayers[resId].playerProfile.name}");

        //接続可能だった場合
        
        List<LobbyPlayerData> currentPlayerList = new List<LobbyPlayerData>();
        foreach (var player in _manager.ServerState.MasterPlayers)
        {
            int id = player.Value.playerProfile.id;
            string name = player.Value.playerProfile.name;
            bool isReady = player.Value.IsReady;
            LobbyPlayerData playerData = new LobbyPlayerData(id,name,isReady);

            currentPlayerList.Add(playerData);
        }

        //接続応答の送信
        var connectResponseData = new ConnectResponseData {roomId = GameSessionData.Instance.roomId, playerId = resId, firstPlayerList = currentPlayerList};
        Debug.Log($"[Check JSON ConnectAnounce]{JsonUtility.ToJson(connectResponseData)}");
        _manager.SendPacketToClient(NetworkCommand.ConnectResponse, connectResponseData, sender);

        //接続者のデータの送信
        var connectPlayerData = new LobbyPlayerData(resId,data.playerName,false);
        foreach (var player in _manager.ServerState.MasterPlayers)
        {
            _manager.SendPacketToClient(NetworkCommand.ConnectAnounce,connectPlayerData, player.Value.clientIP);
        }
    }

}
