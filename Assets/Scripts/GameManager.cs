using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] float limitTime = 60f;
    [SerializeField] float timeCount;
    public bool onGame;
    [SerializeField] int playerSelf = 0;
    [SerializeField] int firstTagger = 1;
    [SerializeField] int whoTagger;
    [SerializeField] GameObject[] players;
    [SerializeField] UIController uIController;
    // Start is called before the first frame update
    void Start()
    {
        whoTagger = firstTagger;
        timeCount = limitTime;
        onGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (onGame)
        {
            if (timeCount < 0)
            {
                onGame = false;
            }
            
            timeCount -= Time.deltaTime;
            uIController.SetPlayerUI(players);
            uIController.SetTimeCountText(timeCount);
            PlayerSelfIsTagger();

            CheckTaggerError();
        }
        else
        {
            uIController.SetGameResultUI(PlayerSelfIsTagger());
        }
    }

    bool PlayerSelfIsTagger()
    {
        PlayerStatusController playerStatusController = players[playerSelf].GetComponent<PlayerStatusController>();
        bool isTagger = playerStatusController.isTagger;

        uIController.SetTaggerUI(isTagger);
        return isTagger;
    }

    void CheckTaggerError()
    {
        int count = 0;
        foreach (GameObject player in players)
        {
            PlayerStatusController playerStatusController = player.GetComponent<PlayerStatusController>();
            bool isTagger = playerStatusController.isTagger;
            if (isTagger)
            {
                count += 1;
            }
        }

        if (count > 1)
        {
            count = 0;
            foreach (GameObject player in players)
            {
                PlayerStatusController playerStatusController = player.GetComponent<PlayerStatusController>();
                bool isTagger = playerStatusController.isTagger;
                if (isTagger)
                {
                    if (count == 0)
                    {
                        count += 1;
                    }
                    else
                    {
                        playerStatusController.isTagger = false;
                    }
                }
            }
        }
    }
}
