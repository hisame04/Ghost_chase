using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;

public interface IGameClient
{
    void Connect(string ip, int port);
    void SendMessage(string message);
    void Close();
    // ConcurrentQueue<string> ReceiveQueue { get; }
}
