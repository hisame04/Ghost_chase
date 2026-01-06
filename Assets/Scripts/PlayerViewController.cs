using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewController : MonoBehaviour
{
    [SerializeField] AvatarData avatarData;
    public PlayerProfile myProfile;

    //変更するマテリアルの保存用変数
    Material bodyBaseMaterial;
    Material earsBaseMaterial;

    void Awake()
    {
        FindMaterial();//最初に子オブジェクトから変更するマテリアルを取得
    }

    /*見た目の初期設定 (SpawnPlayerControllerから呼び出し)*/
    public void Setup()
    {
        Debug.Log($"PlayerViewController Setup /id:{myProfile.id} color:{myProfile.colorIdx}");
        AvatarBaseColorSet(myProfile.colorIdx,this.gameObject);//アバターのベース色を初期設定
    }

    /*変更したいマテリアルの取得*/
    void FindMaterial()
    {
        //bodyパーツとearsパーツが何番目か
        int bodyPartNum = 0;
        int earsPartNum = 1;
        //マテリアルの中でベースとアウトラインが何番目か
        int baseMaterialNum = 0;

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
        earsBaseMaterial = earsMats[baseMaterialNum];
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
}
