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

public static class NetworkUtils
{
    /*自分のローカルIPアドレス(IPv4)を取得する*/
    public static string GetLocalIPAddress()
    {
        // 自分のPCのホスト名（コンピュータ名）を取得
        var hostName = Dns.GetHostName();

        // ホスト名からIPアドレスのリストを取得
        var hostEntry = Dns.GetHostEntry(hostName);

        // リストの中から「InterNetwork (IPv4)」のアドレスを探す
        foreach (var ip in hostEntry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                Debug.Log(ip);
                return ip.ToString(); // 見つかったら文字列として返す (例: "192.168.1.5")
            }
        }

        throw new Exception("IPv4アドレスが見つかりませんでした（ネットワークに接続されていません）");
    }

    /*自分のグローバルIPアドレス(IPv4)を取得する*/
    public static async Task<string> GetGlobalIPAdress()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                string result = await client.GetStringAsync("https://api.ipify.org");
                Debug.Log(result.Trim());
                return result.Trim();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("グローバルIPの取得に失敗しました" + ex.Message);
            return null;
        }
    }
}


[System.Serializable]
/*IPアドレスのデータを登録するためのクラス*/
public class RoomData
{
    public string localIp;
    public string globalIp;
    public int port;

    public RoomData(string localIp, string globalIp, int port)
    {
        this.localIp = localIp;
        this.globalIp = globalIp;
        this.port = port;
    }
}