using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookController : MonoBehaviour
{
    public float mouseSensitivity = 100f;//マウス感度
    public Transform playerBody;// プレイヤーの体（左右回転用）
    private float xRotation = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.onGame)
        {
            MouseLook();
        }
    }

    void MouseLook()
    {
        // マウスの移動量を取得
        // Time.deltaTimeを掛けることでフレームレートに依存しない動きにする
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 縦方向（上下）の回転計算（マウスを上げると視点が上がるように - を使う）
        xRotation -= mouseY;
        
        // 真上・真下を見過ぎないように角度を制限（-90度〜90度）
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // カメラ自体の縦回転を適用
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // プレイヤーの体ごと横回転させる（playerBodyが設定されている場合）
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            // 体がない（カメラ単体）の場合はカメラ自体を横回転させる
            transform.Rotate(Vector3.up * mouseX);
        }
    }
}
