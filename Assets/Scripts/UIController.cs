using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    [SerializeField] TextMeshProUGUI timeCountText;
    [SerializeField] TextMeshProUGUI waitTimeText;
    [SerializeField] GameObject isTaggerText;
    [SerializeField] GameObject touchButton;
    [SerializeField] GameObject gameResultUIPanel;
    [SerializeField] GameObject waitGameUIPanel;
    [SerializeField] GameObject onGameUIPanel;
    [SerializeField] TextMeshProUGUI gameResultText;
    [SerializeField] Button nextGameButton;

    // [SerializeField] int bodyMeshNum = 0;
    // [SerializeField] int earsMeshNum = 1;
    
    //*オブジェクト生成時の処理(全てのAwake前)*//
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //*オブジェクト生成時の処理(大体全てのAwake後)*//
    void Start()
    {
        gameResultUIPanel.SetActive(false);
        waitGameUIPanel.SetActive(true);
        onGameUIPanel.SetActive(false);

        nextGameButton.onClick.AddListener(GoToNextGame);//次のゲームに進む(通信切断&シーン遷移)ボタン

        NetworkManager.Instance.OnTagPlayerChanged += SetTaggerUI;//ClientPacketHandler→NetworkManager→UIControllerで鬼用UIの表示・非表示切り替えのメソッドを呼び出す

        PlayerProfile myProfile = GameSessionData.Instance.GetPlayer(GameSessionData.Instance.myIDNum);
        SetTaggerUI(myProfile.isTag);//鬼のUIの表示・非表示
        gameResultUIPanel.SetActive(false);//結果画面の非表示
    }

    /*オブジェクトが消滅した時の処理*/
    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTagPlayerChanged -= SetTaggerUI;//鬼用UIの表示・非表示切り替えのメソッドを呼び出しを解除
        }
    }

    // Update is called once per frame
    void Update()
    {

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

    //鬼のUIを表示・非表示
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
        NetworkManager.Instance.DisconnectLocal();
        SceneManager.LoadScene("TitleScene");
    }
}
