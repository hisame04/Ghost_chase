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
using System.Net.NetworkInformation;

public static class NetworkUtils
{
    /*自分のローカルIPアドレス(IPv4)を取得する*/
    public static string GetLocalIPAddress()
    {
        try
        {
            //1:ネットワークインターフェース（ハードウェア）から直接探す
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 使われていない(Down)、またはループバック(自分自身)は無視
                if (ni.OperationalStatus != OperationalStatus.Up || 
                    ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                // その接続のIPプロパティを取得
                var ipProps = ni.GetIPProperties();

                // 設定されているユニキャストアドレスを探す
                foreach (var ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string foundIp = ip.Address.ToString();
                        Debug.Log($"Local IP Found (Interface): {foundIp}");
                        return foundIp;
                    }
                }
            }

            //2:もし上記で見つからなかったら、旧来のDNS方式を試す（念のため）
            var hostName = Dns.GetHostName();
            var hostEntry = Dns.GetHostEntry(hostName);
            foreach (var ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    Debug.Log($"Local IP Found (DNS): {ip}");
                    return ip.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"ローカルIP検索中にエラー: {ex.Message}");
        }

        //3:とりあえず動くようにlocalhostを返す
        Debug.LogError("有効なローカルIPが見つかりませんでした。127.0.0.1を使用します。");
        return "127.0.0.1";
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
                Debug.Log($"Local IP Found (DNS): {result.Trim()}");
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