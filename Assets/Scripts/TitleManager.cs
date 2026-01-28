using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameSessionData.Instance.ResetAllState();
        NetworkManager.Instance.ServerState.ResetAllStates();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
