////*TCPのサーバ・クライアントのスクリプト*////
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
using Unity.VisualScripting;

//*サーバのクラス*//
public class TcpGameServer : IGameServer
{
    public Action<string, IPEndPoint> OnDataReceived { get; set; }
    private TcpListener _listener;
    public bool isRunning { get; private set; }

    private Dictionary<IPEndPoint, TcpClient> _connectedClients = new Dictionary<IPEndPoint, TcpClient>();

    /*サーバの立ち上げ*/
    public async Task<bool> StartServer(int port)
    {
        try
        {
            var local = new IPEndPoint(IPAddress.Any, port);
            _listener = new TcpListener(local);
            _listener.Start();
            isRunning = true;
            Debug.Log($"[TCPServer] 起動しました Port:{port}");

            _=AcceptLoop();

            return true;
        }   
        catch (Exception ex)
        {
            Debug.Log($"{ex.Message}");
            return false;
        }
    }

    //受信処理(ループ)
    private async Task AcceptLoop()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.Log($"[Server] Error:{ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        //送信元のIPアドレス
        IPEndPoint remote = client.Client.RemoteEndPoint as IPEndPoint;

        lock (_connectedClients)
        {
            if (_connectedClients.ContainsKey(remote)) _connectedClients.Remove(remote);
            _connectedClients.Add(remote, client);
        }

        //データが流れるストリーム(NetworkStream)の取得
        var stream = client.GetStream();
        stream.ReadTimeout = 5000;

        // ヘッダー読み込み用（4バイト固定）
        var headerBuffer = new byte[4];

        try
        {
            while (client.Connected)
            {
                if (!await ReadExactAsync(stream, headerBuffer, 4)) break;

                //ヘッダー(データの長さ)を読む
                int bodyLength = BitConverter.ToInt32(headerBuffer, 0);//ヘッダーのバイト列をintに変換
                if (bodyLength > 10 * 1024 * 1024 || bodyLength < 0) return;

                //ボディ(データ本体)を読む
                var bodyBuffer = new byte[bodyLength];
                if (!await ReadExactAsync(stream, bodyBuffer, bodyLength)) break;

                // 受信データの処理
                var data = Encoding.UTF8.GetString(bodyBuffer);
                this.OnRecieve(data, remote);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"クライアント処理エラー: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    //指定したバイト数だけきっちり読み取るメソッド
    private async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int size)
    {
        int totalRead = 0;
        while (totalRead < size)
        {
            int read = await stream.ReadAsync(buffer, totalRead, size - totalRead);
            if (read == 0) return false;//切断された場合
            totalRead += read;
        }
        return true;
    }

    /*受信したデータの処理*/
    private void OnRecieve(string data, IPEndPoint remote)
    {
        Debug.Log($"[Server]受信{data} from:{remote}");
        OnDataReceived?.Invoke(data, remote);

    }

    /*サーバからの送信*/
    public void SendMessage(string message, IPEndPoint remote)
    {
        TcpClient client = null;

        //送信先が接続リストにいるか確認
        lock (_connectedClients)
        {
            if (!_connectedClients.TryGetValue(remote, out client))
            {
                Debug.LogWarning($"[TCP Server] 送信先が見つかりません: {remote}");
                return;
            }
        }

        try
        {
            NetworkStream stream = client.GetStream();

            //送信
            var body = Encoding.UTF8.GetBytes(message);//データの内容をバイト列に変換
            var header = BitConverter.GetBytes(body.Length);//データの長さを数値データに変換

            // ヘッダー(データの長さ)+ボディ(データ)を結合して送信
            byte[] packet = new byte[header.Length + body.Length];
            Array.Copy(header, 0, packet, 0, header.Length);
            Array.Copy(body, 0, packet, header.Length, body.Length);

            stream.Write(packet, 0, packet.Length);
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
        _listener.Stop();

        lock (_connectedClients)
        {
            foreach (var client in _connectedClients.Values)
            {
                client.Close();
            }
            _connectedClients.Clear();
        }

        Debug.Log("[Server] 停止しました");
    }
}

//*クライアントのクラス*//
public class TcpGameClient : IGameClient
{
    public Action<string> OnDataReceived { get; set; }
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private IPEndPoint _serverEndPoint;

    /*サーバへの接続*/
    public Task ConnectAsync(string serverIp, int serverPort)
    {
        try
        {
            if (_tcpClient != null) return Task.CompletedTask;//接続済なら何もしない

            _tcpClient = new TcpClient();
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            _tcpClient.Connect(_serverEndPoint);
            _stream = _tcpClient.GetStream();
            _ = ListenMessageAsync();

            Debug.Log($"[Client] サーバに接続しました{((IPEndPoint)_tcpClient.Client.LocalEndPoint).Port}");

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
        if (_tcpClient == null || !_tcpClient.Connected)
        {
            Debug.Log("まだ接続されていません。先に Connect() を呼んでください。");
            return;
        }

        try
        {
            //送信
            var body = Encoding.UTF8.GetBytes(message);//データの内容をバイト列に変換
            var header = BitConverter.GetBytes(body.Length);//データの長さを数値データに変換

            // ヘッダー(データの長さ)+ボディ(データ)を結合して送信
            byte[] packet = new byte[header.Length + body.Length];
            Array.Copy(header, 0, packet, 0, header.Length);
            Array.Copy(body, 0, packet, header.Length, body.Length);

            _stream.Write(packet, 0, packet.Length);
        }
        catch (Exception ex)
        {
            Debug.Log($"送信エラー: {ex.Message}");
        }
    }

    /*メッセージの受信*/
    public async Task ListenMessageAsync()
    {
        byte[] headerBuffer = new byte[4];
        IPEndPoint remote = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;

        if (_tcpClient == null) return;

        try
        {
            while (_tcpClient.Connected)
            {
                if (!await ReadExactAsync(_stream, headerBuffer, 4)) break;

                //ヘッダー(データの長さ)を読む
                int bodyLength = BitConverter.ToInt32(headerBuffer, 0);//ヘッダーのバイト列をintに変換
                if (bodyLength > 10 * 1024 * 1024 || bodyLength < 0) return;

                //ボディ(データ本体)を読む
                var bodyBuffer = new byte[bodyLength];
                if (!await ReadExactAsync(_stream, bodyBuffer, bodyLength)) break;

                // 受信データの処理
                var data = Encoding.UTF8.GetString(bodyBuffer);
                this.OnRecieve(data, remote);
            }
        }
        catch (ObjectDisposedException)
        {
            Debug.Log("[client]受信待機を終了しました。");
        }
        catch (Exception ex)
        {
            if (_tcpClient != null)
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

    //指定したバイト数だけきっちり読み取るメソッド
    private async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int size)
    {
        int totalRead = 0;
        while (totalRead < size)
        {
            int read = await stream.ReadAsync(buffer, totalRead, size - totalRead);
            if (read == 0) return false;//切断された場合
            totalRead += read;
        }
        return true;
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
        _stream?.Close();
        _tcpClient?.Close();
        _tcpClient = null;
        Debug.Log("[Client] 停止しました");
    }

    /*UDPClientのIPアドレスを返すメソッド*/
    public IPEndPoint ClientUseIP()
    {
        return (IPEndPoint)_tcpClient.Client.LocalEndPoint;
    }

}
