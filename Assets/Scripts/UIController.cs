using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class UIController : MonoBehaviour
{
    [SerializeField] Color taggerColor = Color.red;
    [SerializeField] Color playerColor = Color.blue;
    [SerializeField] TextMeshProUGUI timeCountText;
    [SerializeField] TextMeshProUGUI waitTimeText;
    [SerializeField] GameObject isTaggerText;
    [SerializeField] GameObject touchButton;
    [SerializeField] GameObject gameResultUIPanel;
    [SerializeField] GameObject waitGameUIPanel;
    [SerializeField] GameObject onGameUIPanel;
    [SerializeField] TextMeshProUGUI gameResultText;
    // [SerializeField] int bodyMeshNum = 0;
    // [SerializeField] int earsMeshNum = 1;
    [SerializeField] int outLineShaderNum = 1;
    // Start is called before the first frame update
    void Start()
    {
        gameResultUIPanel.SetActive(false);
        waitGameUIPanel.SetActive(true);
        onGameUIPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPlayerUI(GameObject[] players)
    {
        foreach (GameObject player in players)
        {
            PlayerStatusController playerStatusController = player.GetComponent<PlayerStatusController>();
            bool isTagger = playerStatusController.isTagger;
            Color colorToChange = isTagger ? taggerColor : playerColor;

            TextMeshPro tmp = player.GetComponentInChildren<TextMeshPro>();

            GameObject bodyObject = player.transform.Find("body").gameObject;
            GameObject earsObject = player.transform.Find("ears").gameObject;

            if (bodyObject != null) { ChangeOutlineColor(bodyObject, colorToChange); }
            if (earsObject != null) { ChangeOutlineColor(earsObject, colorToChange); }

            if (tmp != null) { ChangeTmp(tmp, colorToChange); }
        }
    }

    void ChangeOutlineColor(GameObject partObject, Color color)
    {
        MeshRenderer meshRenderer = partObject.GetComponent<MeshRenderer>();
        Material[] mats = meshRenderer.materials;
        if (mats.Length >= outLineShaderNum + 1)
        {
            mats[outLineShaderNum].SetColor("_OutlineColor", color);
            meshRenderer.materials = mats;
        }

    }

    void ChangeTmp(TextMeshPro tmp, Color color)
    {
        tmp.color = color;
    }
    //制限時間のUIテキスト変更
    public void SetTimeCountText(float timeCount)
    {
        string timeText = timeCount >=0 ? timeCount.ToString(("F0")) : "0";
        timeCountText.text =timeText;
    }
    //開始待ち時間のUIテキスト変更
    public void SetWaitTimeText(float timeCount)
    {
        // ゲーム開始時にマウスカーソルを非表示にする
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        string timeText = timeCount >= 0.5 ? timeCount.ToString(("F0")) : "Start!";
        waitTimeText.text =timeText;
    }
    //ゲーム開始時のUI切り替え
    public void SetOnGameUI()
    {
        //待機時間のUIを非表示にする
        waitGameUIPanel.SetActive(false);
        //試合中のUIを表示する
        onGameUIPanel.SetActive(true);
    }

    public void SetTaggerUI(bool isTagger)
    {
        if (isTagger)
        {
            isTaggerText.SetActive(true);
            touchButton.SetActive(true);
        }
        else
        {
            isTaggerText.SetActive(false);
            touchButton.SetActive(false);
        }
    }

    public void SetGameResultUI(bool isTagger)
    {
        string gameResult = isTagger ? "Lose..." : "Win!";
        gameResultText.text = gameResult;

        gameResultUIPanel.SetActive(true);

        //マウスカーソルを表示する
        Cursor.visible = true;
    }

    public void GoToNextGame()
    {
        Debug.Log("Go To TitleScene");
        SceneManager.LoadScene("TitleScene");
    }
}
