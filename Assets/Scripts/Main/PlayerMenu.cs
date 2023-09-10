using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
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
        //Отправляем на сервер сигнал завершения работы нашего клиента
        byte[] data = Encoding.UTF8.GetBytes("end");
        MP_Manager.Instance.tcpClient.Send(data);

        //Закрываем соединение tcpClient
        MP_Manager.Instance.tcpClient.Shutdown(SocketShutdown.Both);
        MP_Manager.Instance.tcpClient.Close();

        //Закрываем соединение tcpGameHandler
        MP_Manager.Instance.stream.Close();
        MP_Manager.Instance.tcpGameHandler.Close();

        //Переходим в меню
        ErrorIndicator.errorIndicator.SetEmpty();
        MenuManager.Instance.DisableBtStart();
        MenuManager.Instance.ShowMenu();
        SceneManager.LoadScene(0);
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
