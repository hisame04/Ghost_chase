using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public interface IGameClient
{
    Task ConnectAsync(string ip, int port);
    void SendMessage(string message);
    void Close();
    // ConcurrentQueue<string> ReceiveQueue { get; }
}
