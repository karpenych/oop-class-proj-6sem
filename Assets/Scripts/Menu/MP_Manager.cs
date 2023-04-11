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
    public static MP_Manager Instance { get; private set; }

    [SerializeField] GameObject conn_text_obj;
    [SerializeField] TMP_Text conn_text;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Material myColor;

    public static Dictionary<int, Player> playersInGame;

    const string SERVER_IP = "127.0.0.1";
    const int SERVER_PORT = 8080;
    Socket tcpClient;
    MoveData moveData;

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

    struct MoveData 
    {
        public bool up;
        public bool right;
        public bool down;
        public bool left;
        public int  _id;
    }




    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tcpClient = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        playersInGame = new();
        moveData = new()
        {
            up    = false,
            right = false,
            down  = false,
            left  = false,
            _id   = DataManager.dataManager.userData.playerData.id
        };
        
        ConnectToServer();
    }

    void Update()
    {
        moveData.up = false;
        moveData.right = false;
        moveData.down = false;
        moveData.left = false;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveData.up = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveData.right = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveData.down = true;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveData.left = true;
        }

        if (moveData.up || moveData.right || moveData.down || moveData.left) // ���� ���� �������� ��
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(moveData));
            tcpClient.Send(data); // �������� ���� � ������������ (id � �����������)
        }

    }

    void ConnectToServer()
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
        print("������: " + tcpClient.RemoteEndPoint);
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

        print("��������� �� ������: " + JsonUtility.ToJson(toServerFirstCon));
        byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(toServerFirstCon));
        tcpClient.Send(data); // �������� ���� � ���� (id � ����������)

        data = new byte[4];
        tcpClient.Receive(data); // �������� ���-�� ������� � ����
        var playersAmount = int.Parse(Encoding.UTF8.GetString(data));
        tcpClient.Send(Encoding.UTF8.GetBytes("+")); // ���������� ������������� � ���������
        print($"��������: ���������� ������� � ���� ({playersAmount})");

        for (int i = 0; i < playersAmount; ++i)
        {
            PlayerInfo playerInGameInfo = new() { };

            data = new byte[1024];
            var read_len = tcpClient.Receive(data); // �������� ���� � ��������� ������ � ����

            var player_data = new byte[read_len];
            Array.Copy(data, 0, player_data, 0, read_len);

            var playerInGameInfoJsonStr = Encoding.UTF8.GetString(player_data); // ���������� ����� � JSON
            print($"�������� JSON ������[{i}] - {playerInGameInfoJsonStr}");

            playerInGameInfo = JsonUtility.FromJson<PlayerInfo>(playerInGameInfoJsonStr); // �� JSON � ���������

            var playerPos = new Vector3(playerInGameInfo.X, 3.5f, playerInGameInfo.Z);
            var playerGO = Instantiate(playerPrefab, playerPos, Quaternion.identity); // ������� ������

            Player playerInGame = new()
            {
                _PlayerGameObject = playerGO,
                X = playerInGameInfo.X,
                Z = playerInGameInfo.Z,
            };

            playersInGame.Add(playerInGameInfo.ID, playerInGame); // ��������� ������ � ������

            tcpClient.Send(Encoding.UTF8.GetBytes("+")); // ���������� ������������� � ���������
            print("����� ������� � ���������");
        }

        playersInGame[DataManager.dataManager.userData.playerData.id]._PlayerGameObject.GetComponent<MeshRenderer>().material = myColor; // �������� ���� ������ ������
    }



}
