////*クライアントが持つプレイヤーのデータ*////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{ 
    public int id;          // 識別番号 (0, 1, 2...)
    public GameObject playerObject; //プレイヤーのオブジェクト
    public Vector3 firstSpawnPos; //初期スポーン位置
    public string name;     // 名前 ("Taro")
    public int avatarIdx;   // アバターの種類 (0:Normal, 1:Robot...)
    public int colorIdx;    // 色 (0:Red, 1:Blue...)
    public bool isTag; // 最初鬼かどうか
    public bool isReady; //準備完了かどうか
    public bool isConnectAlive; //接続しているかどうか
    public Vector3 currentPos;//位置
    public Quaternion currentRot;//回転

    // コンストラクタ（あると便利）
    public PlayerProfile(int id, string name, bool isReady)
    {
        this.id = id;
        this.name = name;
        this.isReady = isReady;
    }
}
