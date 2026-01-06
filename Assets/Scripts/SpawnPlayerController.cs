using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayerController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] AvatarData avatarData;
    [SerializeField] Vector3 spawnPosition;

    void Awake()
    {
        SpawnMyAvatar();//自身のアバターの生成
        SpawnEnemyAvatar();//相手のアバターの生成
    }

    /*自身のアバターを作成するメソッド*/
    void SpawnMyAvatar()
    {
        int myIDNum = GameSessionData.Instance.myIDNum;
        var allPlayers = GameSessionData.Instance.AllPlayers;

        int myAvatarIndex = GameSessionData.Instance.myAvatarIndex;
        GameObject mySpawnObject = MyAvatarSpawn(myIDNum,myAvatarIndex,spawnPosition);
        SetAvatarObject(myIDNum,mySpawnObject);
    }
    
    /*相手のアバターを作成するメソッド*/
    void SpawnEnemyAvatar()
    {
        int myIDNum = GameSessionData.Instance.myIDNum;
        var allPlayers = GameSessionData.Instance.AllPlayers;

        foreach(var player in allPlayers)
        {
            if(player.Key !=myIDNum)
            {
                GameObject spawnObject = EnemyAvatarSpawn(player.Key,player.Value.avatarIdx,player.Value.firstSpawnPos);
                SetAvatarObject(player.Key,spawnObject);
            }
        }
    }

    /*自身のアバターの設定するメソッド*/
    GameObject MyAvatarSpawn(int playerID,int avatarIdx,Vector3 spawnPos)
    {
        GameObject avatarPrb = avatarData.playerAvatars[avatarIdx];
        GameObject myAvatar = Instantiate(avatarPrb,spawnPos,Quaternion.identity);

        myAvatar.gameObject.tag = "Player";//タグの設定(当たり判定で必要)
        myAvatar.GetComponent<PlayerMovement>().gameManager = gameManager;//移動のスクリプトの設定
        myAvatar.GetComponent<PlayerStatusController>().gameManager = gameManager;//状態遷移のスクリプトの設定
        myAvatar.GetComponentInChildren<MouseLookController>().gameManager = gameManager;//視点回転のスクリプトの設定

        myAvatar.GetComponentInChildren<MouseLookController>().playerBody = myAvatar.transform;//視点回転のスクリプトの設定

        PlayerViewController playerViewController = myAvatar.GetComponent<PlayerViewController>();
        playerViewController.myProfile = GameSessionData.Instance.GetPlayer(playerID);//PlayerProfileを渡して固有情報を持たせる
        playerViewController.Setup();//色の初期設定
        //StatusControllerにあとでIDをわたす
        
        
        return myAvatar;
    }

    /*相手のアバターの設定をするメソッド*/
    GameObject EnemyAvatarSpawn(int playerID,int avatarIdx,Vector3 spawnPos)
    {
        GameObject avatarPrb = avatarData.enemyAvatars[avatarIdx];
        GameObject myAvatar = Instantiate(avatarPrb,spawnPos,Quaternion.identity);

        myAvatar.gameObject.tag = "Player";//タグの設定(当たり判定で必要)
        //移動のスクリプトの設定
        //状態遷移のスクリプトの設定
        myAvatar.GetComponent<PlayerStatusController>().gameManager = gameManager;//状態遷移のスクリプトの設定

        EnemyViewController enemyViewController = myAvatar.GetComponent<EnemyViewController>();
        enemyViewController.myProfile = GameSessionData.Instance.GetPlayer(playerID);//PlayerProfileを渡して固有情報を持たせる
        enemyViewController.Setup();//色の初期設定

        return myAvatar;
    }

    /*生成したオブジェクトをPlayerProfileにセットするメソッド*/
    void SetAvatarObject(int idNum,GameObject avatarObj)
    {
        var playerProfile = GameSessionData.Instance.GetPlayer(idNum);
        playerProfile.playerObject = avatarObj;
    }

}
