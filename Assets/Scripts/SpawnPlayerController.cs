using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPlayerController : MonoBehaviour
{
    [SerializeField] AvatarData avatarData;

    void Awake()
    {
        SpawnAllPlayer();
    }

    void SpawnAllPlayer()
    {
        int myIDNum = GameSessionData.Instance.myIDNum;
        foreach(var player in GameSessionData.Instance.AllPlayers)
        {
            if (player.Key != myIDNum)
            {
                GameObject spawnObject = EnemyAvatarSpawn(player.Key,player.Value.avatarIdx,player.Value.firstSpawnPos);
                SetAvatarObject(player.Key,spawnObject);
            }
            else if(player.Key == myIDNum)
            {
                GameObject spawnObject = MyAvatarSpawn(player.Key,player.Value.avatarIdx,player.Value.firstSpawnPos);
                SetAvatarObject(player.Key,spawnObject);
            }
        }
    }

    /*自身のアバターの設定するメソッド*/
    GameObject MyAvatarSpawn(int playerID,int avatarIdx,Vector3 spawnPos)
    {
        GameObject avatarPrb = avatarData.playerAvatars[avatarIdx];
        GameObject myAvatar = Instantiate(avatarPrb,spawnPos,Quaternion.identity);

        SetUpMySpawnObject(playerID,myAvatar);
        
        return myAvatar;
    }

    void SetUpMySpawnObject(int playerId,GameObject myAvatar)
    {
        myAvatar.gameObject.tag = "Player";//タグの設定(当たり判定で必要)

        myAvatar.GetComponentInChildren<MouseLookController>().playerBody = myAvatar.transform;//視点回転のスクリプトの設定

        PlayerViewController playerViewController = myAvatar.GetComponent<PlayerViewController>();
        playerViewController.Init();
        playerViewController.Setup();//色の初期設定
        //StatusControllerにあとでIDをわたす
        StatusController statusController = myAvatar.GetComponent<StatusController>();
        statusController.Init(playerId);
    }

    /*相手のアバターの設定をするメソッド*/
    GameObject EnemyAvatarSpawn(int playerID,int avatarIdx,Vector3 spawnPos)
    {
        GameObject avatarPrb = avatarData.enemyAvatars[avatarIdx];
        GameObject enemyAvatar = Instantiate(avatarPrb,spawnPos,Quaternion.identity);

        SetUpEnemySpawnObject(playerID,enemyAvatar);

        return enemyAvatar;
    }

    void SetUpEnemySpawnObject(int playerId,GameObject enemyAvatar)
    {
        enemyAvatar.gameObject.tag = "Player";//タグの設定(当たり判定で必要)
        //移動のスクリプトの設定
        //状態遷移のスクリプトの設定

        EnemyViewController enemyViewController = enemyAvatar.GetComponent<EnemyViewController>();
        enemyViewController.Init(playerId);
        enemyViewController.Setup();//色の初期設定

        StatusController statusController = enemyAvatar.GetComponent<StatusController>();
        statusController.Init(playerId);//IDを渡す
    }

    /*生成したオブジェクトをPlayerProfileにセットするメソッド*/
    void SetAvatarObject(int idNum,GameObject avatarObj)
    {
        var playerProfile = GameSessionData.Instance.GetPlayer(idNum);
        playerProfile.playerObject = avatarObj;
    }

}
