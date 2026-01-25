////*UDPのサーバ・クライアントのスクリプト*////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Security.Cryptography;

//*サーバのクラス*//
public class UdpGameServer
{
    public Action<string, IPEndPoint> OnDataReceived;
    private UdpClient _udpClient;
    public bool isRunning { get; private set; }

    /*サーバの立ち上げ*/
    public async Task StartServer(int port)
    {
        try
        {
            /*サーバの起動(UdpClientの立ち上げ)*/
            var local = new IPEndPoint(IPAddress.Any, port);
            var remote = new IPEndPoint(IPAddress.Any, 8000);
            _udpClient = new UdpClient(local);
            isRunning = true;
            Debug.Log($"[Server] 起動しました Port:{port}");
        }
        catch (Exception ex)
        {
            Debug.Log($"{ex.Message}");
        }

        while (isRunning)
        {
            try
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                //データ受信待機、受信したらbufferに受信したUDPデータグラムを格納
                var buffer = result.Buffer;
                var remote = result.RemoteEndPoint;
                //受信したデータ(バイト)を文字列に変換
                var data = Encoding.UTF8.GetString(buffer).Replace("\0", "").Trim();
                //空のデータだった場合は処理しない
                if (string.IsNullOrEmpty(data))
                {
                    continue;
                }
                //OnRecieveメソッド(受信イベント)の実行
                this.OnRecieve(data, remote);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Server Receive Error] {ex.Message}");
            }
        }
    }


    /*受信したデータの処理*/
    private void OnRecieve(string data, IPEndPoint remote)//dataはあとで構造体に変更
    {
        Debug.Log($"[Server]受信{data} from:{remote}");
        OnDataReceived?.Invoke(data, remote);

    }

    /*サーバからの送信*/
    public void SendMessage(string message, IPEndPoint remote)
    {
        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            _udpClient.Send(data, data.Length, remote);
        }
        catch (Exception ex)
        {
            Debug.Log($"送信エラー: {ex.Message}");
        }
    }

    /*サーバの停止*/
    public void Stop()
    {
        isRunning = false;
        _udpClient?.Close();
        _udpClient = null;
        Debug.Log("[Server] 停止しました");
    }

}

//*クライアントのクラス*//
public class UdpGameClient : IGameClient
{
    public Action<string> OnDataReceived { get; set; }
    private UdpClient _udpClient;
    private IPEndPoint _serverEndPoint;

    /*サーバへの接続*/
    public Task ConnectAsync(string serverIp, int serverPort)
    {

        try
        {
            if (_udpClient != null) return Task.CompletedTask;//接続済なら何もしない

            _udpClient = new UdpClient(0);
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            _udpClient.Connect(_serverEndPoint);
            _ = ListenMessageAsync();

            Debug.Log($"[Client] サーバに接続しました{((IPEndPoint)_udpClient.Client.LocalEndPoint).Port}");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Client Error] {ex.Message}");
            return Task.CompletedTask;
        }
    }

    /*メッセージの送信*/
    public void SendMessage(string message)
    {
        //接続されていない場合
        if (_udpClient == null)
        {
            Debug.Log("まだ接続されていません。先に Connect() を呼んでください。");
            return;
        }

        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            _udpClient.Send(data, data.Length);//データの送信、第一引数で送信するデータ、第二引数で送信するデータの長さを指定
        }
        catch (Exception ex)
        {
            Debug.Log($"送信エラー: {ex.Message}");
        }
    }

    /*メッセージの受信*/
    public async Task ListenMessageAsync()
    {
        if (_udpClient == null) return;

        try
        {
            while (_udpClient != null)
            {
                var result = await _udpClient.ReceiveAsync();
                //データ受信待機、受信したらbufferに受信したUDPデータグラムを格納
                var buffer = result.Buffer;
                var remote = result.RemoteEndPoint;

                //受信したデータ(バイト)を文字列に変換
                string response = Encoding.UTF8.GetString(buffer).Replace("\0", "").Trim();

                //空のデータだった場合は処理しない
                if (string.IsNullOrEmpty(response))
                {
                    continue;
                }

                this.OnRecieve(response, remote);
            }
        }
        catch (ObjectDisposedException)
        {
            Debug.Log("[client]受信待機を終了しました。");
        }
        catch (Exception ex)
        {
            if (_udpClient != null)
            {
                Debug.Log($"[client]エラーが発生しました: {ex.Message}");
            }
            else
            {
                Debug.Log($"[client]クライアントが閉じられました");
            }
            throw;
        }
    }

    /*受信したデータの処理*/
    private void OnRecieve(string data, IPEndPoint remote)//dataはあとで構造体に変更
    {
        Debug.Log($"[Client]受信{data} from:{remote}");
        OnDataReceived?.Invoke(data);
    }

    /*終了処理*/
    public void Close()
    {
        _udpClient?.Close();
        _udpClient = null;
        Debug.Log("[Client] 停止しました");
    }

    /*UDPClientのIPアドレスを返すメソッド*/
    public IPEndPoint ClientUseIP()
    {
        return (IPEndPoint)_udpClient.Client.LocalEndPoint;
    }

}
