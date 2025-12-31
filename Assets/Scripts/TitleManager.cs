using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] SelectAvatarManager selectAvatarManager;
    void Start()
    {
        
    }

    public void OpenStageScene()
    {
        SceneManager.LoadScene("Stage1");
        GameSessionData.Instance.SetMyAvatar(selectAvatarManager.currentAvatarIndex,selectAvatarManager.currentColorIndex);
    }
    
}
