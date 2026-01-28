using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouch : MonoBehaviour
{
    StatusController statusController;
    int playerId;
    private StatusController currentTargetStatus = null;
    // Start is called before the first frame update
    void Start()
    {
        statusController = this.GetComponent<StatusController>();
        playerId = GameSessionData.Instance.myIDNum;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(statusController != null &&statusController.isTag && currentTargetStatus != null)
            {
                TouchReportData touchData = new TouchReportData{playerId = this.playerId, collisionId = currentTargetStatus.playerId};
                NetworkManager.Instance.SendPacketToHost(NetworkCommand.Touch, touchData);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("touchTag");
            StatusController target = collision.gameObject.GetComponent<StatusController>();
            if(target != null)
            {
                currentTargetStatus = target;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(currentTargetStatus != null && collision.gameObject == currentTargetStatus.gameObject)
            {
                currentTargetStatus = null;
            }
        }
    }
}
