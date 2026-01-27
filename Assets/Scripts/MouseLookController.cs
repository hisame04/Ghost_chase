using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookController : MonoBehaviour
{
    public float mouseSensitivity = 0.03f;//マウス感度
    public Transform playerBody;// プレイヤーの体（左右回転用）
    public Transform cameraTransform;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 lookInput;
    public Rigidbody playerRb;

    void Start()
    {
        if(playerBody != null)
        {
            yRotation = playerBody.rotation.eulerAngles.y;
        }
        if(cameraTransform == null)
        {
            cameraTransform = transform;
        }
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.onGame)
        {
            CalcInput();
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.onGame)
        {
            ApplyRotation();
        }
    }

    void CalcInput()
    {
        // マウスの移動量を取得
        // Time.deltaTimeを掛けることでフレームレートに依存しない動きにする
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // 縦方向（上下）の回転計算（マウスを上げると視点が上がるように - を使う）
        xRotation -= mouseY;
        
        // 真上・真下を見過ぎないように角度を制限（-90度〜90度）
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation += mouseX;
    }

    void ApplyRotation()
    {
        if(playerBody != null && playerRb != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0f,yRotation,0f);
            playerRb.MoveRotation(targetRotation);
        }
        else if(playerBody != null)
        {
            playerBody.localRotation = Quaternion.Euler(0f,yRotation,0f);
        }
        else
        {
            // 体がない（カメラ単体）の場合はカメラ自体を横回転させる
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}
