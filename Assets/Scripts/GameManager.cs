using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float limitTime;
    [SerializeField] float timeCount;
    public bool onGame;
    public bool onWait;
    [SerializeField] GameObject[] players;
    [SerializeField] UIController uIController;
    // Start is called before the first frame update

    void Awake()
    {
        if (Instance == null)
        {
            //インスタンスを自身に設定
            Instance = this;
        }
        else
        {
            //重複防止
            Destroy(gameObject);
        }
    }

    void Start()
    {
        limitTime = GameSessionData.Instance.gameTimeLimit;

        timeCount = GameSessionData.Instance.waitStartTime;
        onWait = false;
        onGame = false;

        NetworkManager.Instance.OnStartCountDown += SetCountWaitTime;

        SendSceneLoaded();
    }

    // Update is called once per frame
    void Update()
    {
        if (onWait)
        {
            if (timeCount < -1)
            {
                onWait = false;
                onGame = true;
                timeCount = limitTime;
                uIController.SetOnGameUI();
                Debug.Log("[client]　ゲームスタート");
            }

            timeCount -= Time.deltaTime;
            uIController.SetWaitTimeText(timeCount);
        }
        else
        {
            if (onGame)
            {
                if (timeCount < 0)
                {
                    onGame = false;
                }

                timeCount -= Time.deltaTime;
                uIController.SetTimeCountText(timeCount);
            }
        }

    }

    //カウントダウン開始フラグのセット
    public void SetCountWaitTime()
    {
        onWait = true;
        Debug.Log("[client] カウントダウンスタート");
    }

    void CheckTaggerError()
    {
        int count = 0;
        foreach (GameObject player in players)
        {
            StatusController statusController = player.GetComponent<StatusController>();
            bool isTag = statusController.isTag;
            if (isTag)
            {
                count += 1;
            }
        }

        if (count > 1)
        {
            count = 0;
            foreach (GameObject player in players)
            {
                StatusController statusController = player.GetComponent<StatusController>();
                bool isTagger = statusController.isTag;
                if (isTagger)
                {
                    if (count == 0)
                    {
                        count += 1;
                    }
                    else
                    {
                        statusController.isTag = false;
                    }
                }
            }
        }
    }

    /*シーン読み込み完了を送るためのメソッド*/
    void SendSceneLoaded()
    {
        var data = new LoadSuccessData{id = GameSessionData.Instance.myIDNum, loadScene = "GameScene"};
        NetworkManager.Instance.SendPacketToHost(NetworkCommand.LoadSuccess,data);
        Debug.Log("[Client] シーン遷移完了通知の送信");
    }

    /*ホストが時間送信時に時間取得のために呼び出すメソッド*/
    public float GetCurrentTime()
    {
        return timeCount;
    }

    /*ホストから受信した時間と合わせるためのメソッド*/
    public void RemainTime(float hostTime)
    {
        if(Mathf.Abs(timeCount -hostTime) > 0.5f)
        {
            timeCount = hostTime;
        }
    }
}
