using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class SelectAvatarManager : MonoBehaviour
{
    [SerializeField] AvatarData avatarData;//
    [SerializeField] Vector3 spawnPoint;// アバターを表示させる場所
    [SerializeField] Vector3 spawnAngle;
    private GameObject[] avatars;
    private Color[] baseColors;
    private Color[] emissionColors;
    private float[] emissionIntensities;
    public int currentAvatarIndex = 0;//現在のアバターのインデックス
    public int currentColorIndex = 0;//現在の色のインデックス
    private GameObject existAvatar;//画面に表示されているアバターオブジェクト
    
    // Start is called before the first frame update
    void Start()
    {
        avatars = avatarData.baseAvatars;
        baseColors = avatarData.baseColors;
        emissionColors = avatarData.emissionColors;
        emissionIntensities = avatarData.emissionIntensity;
        currentAvatarIndex=0;
        currentColorIndex=0;
        ChangeAvatar(currentAvatarIndex);
    }

    // --- アバター（プレハブ）の切り替え ---
    public void NextAvatar() => ChangeAvatar(1);//次のアバターへのボタン用
    public void PrevAvatar() => ChangeAvatar(-1);//前のアバターへのボタン用

    private void ChangeAvatar(int selectIndex)//アバターの種類を選択中のインデックスに変更
    {
        currentAvatarIndex = (currentAvatarIndex + selectIndex + avatars.Length) % avatars.Length;
        GameObject selectAvatar = avatars[currentAvatarIndex];
        if (existAvatar != null)
        {
            Destroy(existAvatar);
            existAvatar =Instantiate (selectAvatar, spawnPoint, Quaternion.identity);
            existAvatar.transform.rotation = Quaternion.Euler(spawnAngle);
            ChangeColor(currentColorIndex);
        }
        else
        {
            existAvatar =Instantiate (avatars[0], spawnPoint, Quaternion.identity);
            existAvatar.transform.rotation = Quaternion.Euler(spawnAngle);
            ChangeColor(currentColorIndex);
        }

        GameSessionData.Instance.SetMyAvatarIdx(currentAvatarIndex);
    }

    // --- 色の切り替え --- 
    public void ChangeColor(int selectIndex)//アバターの色を選択中のインデックスに変更
    {
        Renderer[] avatarRenderers = existAvatar.GetComponentsInChildren<Renderer>();
        Renderer bodyRenderer = avatarRenderers[0];
        Renderer earsRenderer = avatarRenderers[1];
        Color baseColor = baseColors[selectIndex];
        Color emissionColor = emissionColors[selectIndex];
        float emissionIntensity = Mathf.Pow(2,emissionIntensities[selectIndex]);

        bodyRenderer.material.SetColor("_Color", baseColor);
        bodyRenderer.material.SetColor("_EmissionColor", emissionColor*emissionIntensity);
        earsRenderer.material.SetColor("_Color", baseColor);
        earsRenderer.material.SetColor("_EmissionColor", emissionColor*emissionIntensity);

        Debug.Log(emissionIntensity);
        
        currentColorIndex = selectIndex;

        GameSessionData.Instance.SetMyColorIdx(currentColorIndex);
    }

    
}
