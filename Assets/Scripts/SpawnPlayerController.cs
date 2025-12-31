using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayerController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] AvatarData avatarData;
    [SerializeField] Vector3 spawnPosition;
    [SerializeField] int tagPlayerIdx;
    void Awake()
    {
        SpawnMyAvatar();
    }

    void SpawnMyAvatar()
    {
        int myAvatarIndex = GameSessionData.Instance.myAvatarIndex;
        int myColorIndex =GameSessionData.Instance.myColorIndex;

        GameObject myAvatar=AvatarSpawn(myAvatarIndex,spawnPosition);
        AvatarColorSet(myColorIndex,myAvatar);
    }

    GameObject AvatarSpawn(int avatarIdx,Vector3 spawnPos)
    {
        GameObject avatarPrb = avatarData.playerAvatars[avatarIdx];
        GameObject myAvatar = Instantiate(avatarPrb,spawnPos,Quaternion.identity);
        
        myAvatar.gameObject.tag = "Player";
        myAvatar.GetComponent<PlayerMovement>().gameManager = gameManager;
        myAvatar.GetComponent<PlayerStatusController>().gameManager = gameManager;
        myAvatar.GetComponentInChildren<MouseLookController>().gameManager = gameManager;
        myAvatar.GetComponentInChildren<MouseLookController>().playerBody = myAvatar.transform;

        return myAvatar;
    }

    void AvatarColorSet(int colorIdx,GameObject myAvatar)
    {
        Renderer[] avatarRenderers = myAvatar.GetComponentsInChildren<Renderer>();
        Renderer bodyRenderer = avatarRenderers[0];
        Renderer earsRenderer = avatarRenderers[1];
        Color baseColor = avatarData.baseColors[colorIdx];
        Color emissionColor = avatarData.emissionColors[colorIdx];
        float emissionIntensity = Mathf.Pow(2,avatarData.emissionIntensity[colorIdx]);

        bodyRenderer.material.SetColor("_Color", baseColor);
        bodyRenderer.material.SetColor("_EmissionColor", emissionColor*emissionIntensity);
        earsRenderer.material.SetColor("_Color", baseColor);
        earsRenderer.material.SetColor("_EmissionColor", emissionColor*emissionIntensity);
    }
}
