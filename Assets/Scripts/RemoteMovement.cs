using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMovement : MonoBehaviour
{
    private Vector3 targetPos;
    private Quaternion targetRot = Quaternion.identity;
    private float _smoothSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;
    }

    /*受信時に目的の位置・回転をセット*/
    public void OnReceiveMoveData(Vector3 newPos, Quaternion newRot)
    {
        targetPos = newPos;
        targetRot = newRot;
        Debug.Log("updatePos");
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * _smoothSpeed);

        if (targetRot.w == 0 && targetRot.x == 0 && targetRot.y == 0 && targetRot.z == 0)
        {
            return;
        }

        this.transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * _smoothSpeed);
    }
}
