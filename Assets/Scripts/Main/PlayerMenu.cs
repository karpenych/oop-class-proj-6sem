using System;
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
        var pos = MP_Manager.Instance.playersInGame[DataManager.dataManager.userData.playerData.id].transform.position;
        pos.x = (float)Math.Round(pos.x);
        pos.z = (float)Math.Round(pos.z);
        DataManager.dataManager.userData.playerData.pos = JsonUtility.ToJson(pos);
        DataManager.dataManager.SaveData();
        print($"Сохранили координаты: {DataManager.dataManager.userData.playerData.pos}");
    }
}
