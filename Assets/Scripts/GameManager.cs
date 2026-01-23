using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] float limitTime = 60f;
    [SerializeField] float waitStartTime = 3f;
    [SerializeField] float timeCount;
    public bool onGame;
    public bool onWait;
    [SerializeField] int firstTagger = 1;
    [SerializeField] int whoTagger;
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
        whoTagger = firstTagger;
        timeCount = waitStartTime;
        onWait = true;
        onGame = false;
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
}
