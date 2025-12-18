using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    public GameManager gameManager;
    private Vector2 moveInput;

    // InputSystemのPlayerInputから「Move」アクションが呼ばれると実行される
    void OnMove(InputValue value)
    {
        // 入力値を Vector2 (x, y) として受け取る
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        if (gameManager.onGame)
        {
            // 現在の座標に、入力方向 * 速度 * フレーム時間を足す
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            transform.position += movement * speed * Time.deltaTime;
        }
    }
}