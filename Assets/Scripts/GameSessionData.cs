using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; private set; }
    public int myAvatarIndex = 0; 
    public int myColorIndex = 0;
    public string myName = "hisame";
    public int[] enemyAvatarIndex;
    public int[] enemyColorIndex;
    public string[] enemyNames;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても消えないようにする
        }
        else
        {
            Destroy(gameObject); // 重複して存在しないようにする
        }
    }
    public void SetMyAvatar(int avatarIdx, int colorIdx)
    {
        myAvatarIndex = avatarIdx;
        myColorIndex = colorIdx;
    }
}
