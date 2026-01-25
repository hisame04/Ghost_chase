using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouch : MonoBehaviour
{
    StatusController statusController;
    int playerId;
    // Start is called before the first frame update
    void Start()
    {
        statusController = this.GetComponent<StatusController>();
        playerId = GameSessionData.Instance.myIDNum;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("touchTag");
            StatusController collisionStatusController = collision.gameObject.GetComponent<StatusController>();
            if(collisionStatusController == null)
            {
                Debug.Log("[PlayerTouch] タッチ相手のStatusControllerが見つかりません");
                return;
            }
            if(statusController == null)
            {
                Debug.Log($"[PlayerTouch] 自身のStatusControllerが見つかりません");
                return;
            }

            
            if(statusController.isTag){
            // if (Input.GetMouseButtonDown(0)) //鬼が有利そうならオンにする
            // {
                TouchReportData touchData = new TouchReportData{playerId = this.playerId,collisionId = collisionStatusController.playerId};
                NetworkManager.Instance.SendPacketToHost(NetworkCommand.Touch,touchData);
                Debug.Log($"[touch]id:{collisionStatusController.playerId}にタッチ ホストに送信");
            // }
            }
        }
    }
}
