////*UDP/TCPクライアント共通で継承するクラスのスクリプト*////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public interface IGameClient
{
    Action<string> OnDataReceived { get; set; }
    Task ConnectAsync(string ip, int port);
    void SendMessage(string message);
    void Close();
    IPEndPoint ClientUseIP();
    // ConcurrentQueue<string> ReceiveQueue { get; }
}
