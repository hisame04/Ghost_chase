using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI; 

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance; // どこからでもアクセスできるように
    public InputField roomIdInput; // 部屋番号を入力する欄
    public Button hostButton;
    public Button clientButton;

    private UdpGameServer _udpServer;
    
    private IGameClient _client;

    void Awake()
    {
        Instance = this;
    }
}
