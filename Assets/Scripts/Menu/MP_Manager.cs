using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MP_Manager : MonoBehaviour
{
    [SerializeField] GameObject conn_text_obj;
    [SerializeField] TMP_Text conn_text;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Material myColor;

    const string SERVER_IP = "127.0.0.1";
    const int SERVER_PORT = 8080;
    Socket tcpClient;
    public Dictionary<int, Player> playersInGame;


    public struct Player
    {
        public GameObject _PlayerGameObject;
        public float X;
        public float Z;
    }

    struct PlayerInfo
    {
        public int ID;
        public float X;
        public float Z;
    }



    void Start()
    {
        tcpClient = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        playersInGame = new();
        BecomeTCP_Work();
    }

    void Update()
    {


    }

    void BecomeTCP_Work()
    {
        try
        {
            tcpClient.Connect(SERVER_IP, SERVER_PORT);
        }
        catch
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif  
        }
        print("Сервер: " + tcpClient.RemoteEndPoint);
        conn_text_obj.SetActive(false);

        PlayerInfo toServerFirstCon = new()
        {
            ID = DataManager.dataManager.userData.playerData.id,
        };

        if (DataManager.dataManager.userData.playerData.pos == "Null")
        {
            toServerFirstCon.X = 0;
            toServerFirstCon.Z = 0;
        }
        else
        {
            var pos = JsonUtility.FromJson<Vector3>(DataManager.dataManager.userData.playerData.pos);
            toServerFirstCon.X = pos.x;
            toServerFirstCon.Z = pos.z;
        }

        print("Send: " + JsonUtility.ToJson(toServerFirstCon));
        byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(toServerFirstCon));
        tcpClient.Send(data); // Отсылаем инфу о себе (id и координаты)

        data = new byte[4];
        tcpClient.Receive(data); // Получаем кол-во игроков в игре
        var playersAmount = int.Parse(Encoding.UTF8.GetString(data));
        tcpClient.Send(Encoding.UTF8.GetBytes("+")); // Отправляем подтверждение о получении
        print($"Получено: Количество игроков в игре ({playersAmount})");

        for (int i = 0; i < playersAmount; ++i)
        {
            PlayerInfo playerInGameInfo = new() { };

            data = new byte[1024];
            var read_len = tcpClient.Receive(data); // Получаем инфу о очередном игроке в игре

            var player_data = new byte[read_len];
            Array.Copy(data, 0, player_data, 0, read_len);

            var playerInGameInfoJsonStr = Encoding.UTF8.GetString(player_data); // Полученные байты в JSON
            print($"JSON игрока - {playerInGameInfoJsonStr}");

            playerInGameInfo = JsonUtility.FromJson<PlayerInfo>(playerInGameInfoJsonStr); // Из JSON в структуру

            var playerPos = new Vector3(playerInGameInfo.X, 3.5f, playerInGameInfo.Z);
            var playerGO = Instantiate(playerPrefab, playerPos, Quaternion.identity); // Спавним игрока

            Player playerInGame = new()
            {
                _PlayerGameObject = playerGO,
                X = playerInGameInfo.X,
                Z = playerInGameInfo.Z,
            };

            playersInGame.Add(playerInGameInfo.ID, playerInGame); // Добавляем игрока в массив

            tcpClient.Send(Encoding.UTF8.GetBytes("+")); // Отправляем подтверждение о получении
            print("Игрок получен и заспавнен");
        }

        playersInGame[DataManager.dataManager.userData.playerData.id]._PlayerGameObject.GetComponent<MeshRenderer>().material = myColor; // Изменить цвет своему игроку
    }



}
