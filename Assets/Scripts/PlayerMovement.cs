using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Vector2 moveInput;
    Vector3 _lastSentPos;
    Quaternion _lastSentRot;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(SendPosition());
    }

    void OnDestroy()
    {

    }

    // InputSystemのPlayerInputから「Move」アクションが呼ばれると実行される
    void OnMove(InputValue value)
    {
        // 入力値を Vector2 (x, y) として受け取る
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.onGame)
        {
            // 現在の座標に、入力方向 * 速度 * フレーム時間を足す
            // Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        }
    }

    IEnumerator SendPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            //前回のデータとの差
            float dist = Vector3.Distance(transform.position, _lastSentPos);
            float angle = Quaternion.Angle(transform.rotation, _lastSentRot);


            if(dist > 0.01f || angle >1.0f)
            {
                var posData = new MoveStateData
                {
                    id = GameSessionData.Instance.myIDNum,
                    pos = transform.position,
                    rot = transform.rotation
                };

                NetworkManager.Instance.SendPacketToHost(NetworkCommand.Move, posData);
                Debug.Log($"[Client] sendPosData:{transform.position},rot:{transform.rotation}");

                _lastSentPos = transform.position;
                _lastSentRot = transform.rotation;
            }
        }
    }
}