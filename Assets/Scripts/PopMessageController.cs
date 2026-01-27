using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopMessageController : MonoBehaviour
{
    public static PopMessageController Instance { get; private set; }
    public GameObject popUpPanel;
    public TMP_Text popMessage;
    public Button closeButton;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);//重複して存在しないようにする
        }
    }

    void Start()
    {
        closeButton.onClick.AddListener(PopUpClose);//ポップアップを閉じるボタン
    }

    /*ポップアップを表示するためのメソッド*/
    public void PopUpShow(string message)
    {
        Debug.Log($"pop{message}");
        popMessage.text = message;
        popUpPanel.SetActive(true);
    }

    /*ポップアップを閉じるためのメソッド*/
    void PopUpClose()
    {
        popUpPanel.SetActive(false);
    }
}
