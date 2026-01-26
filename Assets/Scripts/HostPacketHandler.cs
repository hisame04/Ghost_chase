////*ホストの受信を処理するためのスクリプト*////
/// 送信・データの書き換え以外(UI操作、オブジェクト操作)はイベント経由で別スクリプトで行うこと ////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Xml.Serialization;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Data;
using TMPro;

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
                //クライアント接続処理
                HandleConnectRequest(packet.dataJson, sender);
                break;
            case NetworkCommand.SendAvatarData:
                //クライアントのアバターデータ受信処理
                HandleSendAvatarData(packet.dataJson, sender);
                break;
            case NetworkCommand.Touch:
                //鬼の交代の受信処理
                HandleTouchData(packet.dataJson, sender);
                break;
            case NetworkCommand.Move:
                //クライアントの移動受信処理
                HandleMoveStateData(packet.dataJson, sender);
                break;
            case NetworkCommand.LoadSuccess:
                //クライアントの移動受信処理
                HandleLoadSuccessData(packet.dataJson, sender);
                break;
            case NetworkCommand.Ping:
                //クライアントの生存報告受信処理
                HandlePingData(packet.dataJson, sender);
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
            bool isReady = player.Value.isReady;
            LobbyPlayerData playerData = new LobbyPlayerData(id,name,isReady);

            currentPlayerList.Add(playerData);
        }

        //接続応答の送信
        var connectResponseData = new ConnectResponseData {roomId = GameSessionData.Instance.roomId, playerId = resId, firstPlayerList = currentPlayerList};
        _manager.SendPacketToClient(NetworkCommand.ConnectResponse, connectResponseData, sender);

        //接続者のデータの送信
        var connectPlayerData = new LobbyPlayerData(resId,data.playerName,false);
        _manager.BroadPacketToAllClient(NetworkCommand.ConnectAnnounce,connectPlayerData);
        
    }

    private void HandleSendAvatarData(string dataJson, IPEndPoint sender)
    {
        //受け取ったデータをアバターデータのクラスに戻す
        SendAvatarData data = JsonUtility.FromJson<SendAvatarData>(dataJson);

        PlayerProfile player =_manager.ServerState.GetPlayer(data.id);//データのidと同じプレイヤーを探す

        if (player != null)
        {
            _manager.ServerState.SetReady(data.id, data.isReady);
            _manager.ServerState.SetAvatarData(data.id, data.avatarIdx, data.colorIdx);

            //準備完了が何人かのチェック&UIの更新
            bool canStart = _manager.ServerState.CheckReadyNum();
            _manager.TriggerHostAllReady(canStart);

            _manager.BroadPacketToAllClient(NetworkCommand.ReadyAnnounce,new LobbyPlayerData(player.id, player.name, player.isReady));

            Debug.Log($"[Host]準備完了(アバターデータ更新) id:{player.id} avatarIdx:{player.avatarIdx} colorIdx:{player.colorIdx}");
        }
    }

    private void HandleLoadSuccessData(string dataJson, IPEndPoint Sender)
    {
        //受け取ったデータをロードサクセスデータのクラスに戻す
        LoadSuccessData data = JsonUtility.FromJson<LoadSuccessData>(dataJson);

        MasterPlayerData player = _manager.ServerState.MasterPlayers[data.id];
        if(player != null)
        {
            if(data.loadScene == "GameScene")
            {
                _manager.ServerState.SetLoadSuccess(data.id, true);//マスターデータの書き換え

                if (_manager.ServerState.CheckAlllLoadSuccess())//全員読み込み完了なら
                {
                    var startCountData = new StartCounDownData{countTime = GameSessionData.Instance.waitStartTime};
                    _manager.BroadPacketToAllClient(NetworkCommand.StartCountDown,startCountData);
                }
            }
        }
    }

    private void HandleTouchData(string dataJson, IPEndPoint sender)
    {
        //受け取ったデータをタッチレポートデータのクラスに戻す
        TouchReportData data = JsonUtility.FromJson<TouchReportData>(dataJson);

        if (!_manager.ServerState.MasterPlayers.ContainsKey(data.playerId))//タッチしたプレイヤーがいなければ
        {
            Debug.Log("[Host] タッチした鬼のデータは存在しません");
            return;
        }
        //IPチェックは回線変更の場合機能しないので一旦なし
        // if(_manager.ServerState.MasterPlayers[data.playerId].clientIP != sender)//送信元とタッチしたプレイヤーが一致しなければ
        // {
        //     Debug.Log($"[Host] 送信元と鬼が一致しません {_manager.ServerState.MasterPlayers[data.playerId].clientIP}:{sender} ");
        //     return;
        // }
        if(_manager.ServerState.GetPlayer(data.collisionId) == null)//タッチした相手がいなければ
        {
            Debug.Log("[Host] collisionPlayerがいません");
            return;
        }

        _manager.ServerState.UpdatePlayerIsTag(data.playerId, false);//タッチした鬼のマスターデータのフラグ(isTag)を更新
        _manager.ServerState.UpdatePlayerIsTag(data.collisionId, true);//タッチされた側のマスターデータのフラグ(isTag)を更新


        List<TagStateData> tagStates = new List<TagStateData>();
        foreach(var player in _manager.ServerState.MasterPlayers)
        {
            TagStateData playerData = new TagStateData(player.Key, player.Value.playerProfile.isTag);
            tagStates.Add(playerData);
        }

        TagChangeData tagList = new TagChangeData("touch",tagStates);

        //アクティブタイムの更新
        _manager.ServerState.UpdateLastActiveTime(data.playerId);

        _manager.BroadPacketToAllClient(NetworkCommand.TagChange,tagList);

        Debug.Log($"[Host] id:{data.collisionId}の人が鬼に交代したことを通知");
    }

    private void HandleMoveStateData(string dataJson, IPEndPoint sender)
    {
        //受け取ったデータをムーブステートデータのクラスに戻す
        MoveStateData data = JsonUtility.FromJson<MoveStateData>(dataJson);

        _manager.ServerState.UpdatePosition(data.id,data.pos);
        _manager.ServerState.UpdateRotation(data.id,data.rot);

        //アクティブタイムの更新
        _manager.ServerState.UpdateLastActiveTime(data.id);
    }

    private void HandlePingData(string dataJson,IPEndPoint sender)
    {
        //受け取ったデータをPingデータのクラスに戻す
        PingData data = JsonUtility.FromJson<PingData>(dataJson);
        
        //アクティブタイムの更新
        _manager.ServerState.UpdateLastActiveTime(data.id);
    }

}
