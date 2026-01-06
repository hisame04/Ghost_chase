using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyViewController : MonoBehaviour
{
    [SerializeField] AvatarData avatarData;
    public PlayerProfile myProfile;
    
    //変更するマテリアルの保存用変数
    Material bodyBaseMaterial;
    Material bodyOutlineMaterial;
    Material earsBaseMaterial;
    Material earsOutlineMaterial;

    //プレイヤー名のTextMesh
    TextMeshPro playerNameText;

    void Awake()
    {
        FindPlayerNameText();
        FindMaterial();//最初に子オブジェクトから変更するマテリアルを取得
    }

    /*見た目の初期設定 (SpawnPlayerControllerから呼び出し)*/
    public void Setup()
    {
        Debug.Log($"EnemyViewController Setup /id:{myProfile.id} color:{myProfile.colorIdx}");
        AvatarBaseColorSet(myProfile.colorIdx,this.gameObject);//アバターのベース色を初期設定
        PlayerNameTextSet(myProfile.name);
        PlayerNameColorSet(myProfile.isTag);
    }

    public void Update()
    {
        AvatarOutlineColorSet(myProfile.isTag);//アウトライン色の更新
        PlayerNameColorSet(myProfile.isTag);//名前色の更新
    }

    /*変更したいマテリアルの取得*/
    void FindMaterial()
    {
        //bodyパーツとearsパーツが何番目か
        int bodyPartNum = 0;
        int earsPartNum = 1;
        //マテリアルの中でベースとアウトラインが何番目か
        int baseMaterialNum = 0;
        int outlineMaterialNum = 1;

        //子オブジェクトからRendererを取得
        Renderer[] avatarRenderers = this.GetComponentsInChildren<Renderer>();
        //取得したRendrerからbodyとearsのRendererを取得
        Renderer bodyRenderer = avatarRenderers[bodyPartNum];
        Renderer earsRenderer = avatarRenderers[earsPartNum];
        //bodyとearsのRenderer
        Material[] bodyMats = bodyRenderer.materials;
        Material[] earsMats = earsRenderer.materials;

        //グローバル変数に保存
        bodyBaseMaterial = bodyMats[baseMaterialNum];
        bodyOutlineMaterial = bodyMats[outlineMaterialNum];
        earsBaseMaterial = earsMats[baseMaterialNum];
        earsOutlineMaterial = earsMats[outlineMaterialNum];
        Debug.Log("finish FindMaterial in EnemyViewController");
    }

    /*プレイヤー名のオブジェクトの取得*/
    void FindPlayerNameText()
    {
        TextMeshPro playerName =this.GetComponentInChildren<TextMeshPro>();
        playerNameText = playerName;
    }

    //アバターのベース色を変更するメソッド
    void AvatarBaseColorSet(int colorIdx,GameObject myAvatar)
    {
        Color baseColor = avatarData.baseColors[colorIdx];
        Color emissionColor = avatarData.emissionColors[colorIdx];
        float emissionIntensity = Mathf.Pow(2,avatarData.emissionIntensity[colorIdx]);

        bodyBaseMaterial.SetColor("_Color", baseColor);
        bodyBaseMaterial.SetColor("_EmissionColor", emissionColor*emissionIntensity);
        earsBaseMaterial.SetColor("_Color", baseColor);
        earsBaseMaterial.SetColor("_EmissionColor", emissionColor*emissionIntensity);
    }

    //アバターのアウトライン色を変更するメソッド
    void AvatarOutlineColorSet(bool isTag)
    {
        Color outlineColor;
        if (isTag)
        {
            outlineColor = avatarData.tagColor;
        }
        else
        {
            outlineColor = avatarData.normalColor;
        }
        

        bodyOutlineMaterial.SetColor("_OutlineColor", outlineColor);
        earsOutlineMaterial.SetColor("_OutlineColor", outlineColor);
    }

    void PlayerNameTextSet(string name)
    {
        playerNameText.text = name;
    }

    void PlayerNameColorSet(bool isTag)
    {
        Color textColor;
        if (isTag)
        {
            textColor = avatarData.tagColor;
        }
        else
        {
            textColor = avatarData.normalColor;
        }
        playerNameText.color = textColor;
    }
}
