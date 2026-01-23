using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    public bool isTag;
    public int playerId;
    public PlayerProfile playerProfile;

    public void Init(int id)
    {
        this.playerId = id;
        this.playerProfile = GameSessionData.Instance.GetPlayer(playerId);
        this.isTag = playerProfile.isTag;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.playerProfile = GameSessionData.Instance.GetPlayer(playerId);
        this.isTag = playerProfile.isTag;

        UpdateTagUI(this.isTag);
    }

    void UpdateTagUI(bool isTag)
    {
        UIController.Instance.SetTaggerUI(isTag);
    }
}
