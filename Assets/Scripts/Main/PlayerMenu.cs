using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : MonoBehaviour
{
    [SerializeField] GameObject playerMenuCanvas;
    [SerializeField] Transform player_pos;


    public void OnResumeClick()
    {
        playerMenuCanvas.SetActive(false);
    }

    public void OnMenuClick()
    {
        MenuManager.Instance.ShowMenu();
        SceneManager.LoadScene(0);
        ErrorIndicator.errorIndicator.SetEmpty();
    }

    public void OnSaveCoordinatesClick()
    {
        DataManager.dataManager.userData.playerData.pos = JsonUtility.ToJson(player_pos.position);
        DataManager.dataManager.SaveData();
    }
    
    
}
