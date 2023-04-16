using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MP_Manager : MonoBehaviour
{
    public static MP_Manager Instance { get; private set; }

    [SerializeField] GameObject playerPrefab; //Prefab ������
    [SerializeField] Material myColor; //���� ������ ������
    [SerializeField] GameObject playerMenu; //����
    [SerializeField] TMP_Text posText; //���� ����������
    float speed = 10f; //�������� ������

    public Dictionary<int, GameObject> playersInGame; //��� ������ � ����
    public Socket tcpClient; //tcp �����������
    public TcpClient tcpGameHandler; //tcp �����������
    public NetworkStream stream; //����� ��� ������ tcp �����������

    const string SERVER_IP = "127.0.0.1"; //IP �������
    const int SERVER_PORT = 8080; //���� �����������
    GameObject myPlayer; //��� �����
    MoveDataTo moveData; //������ � ��������


    struct PlayerInfo //��� ������� 
    {
        public int ID;
        public float X;
        public float Z;
    }

    struct MoveDataTo //��� �������� (��������� �� ������) 
    {
        public bool  Up;
        public bool  Right;
        public bool  Down;
        public bool  Left;
        public int   ID;
        public float X;
        public float Z;
    }

    struct MoveDataFrom //��� �������� (������� �� �������)
    {
        public bool Up;
        public bool Right;
        public bool Down;
        public bool Left;
        public int  ID;
    }




    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tcpClient = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpGameHandler = new(); 

        playersInGame = new();
        moveData = new()
        {
            Up    = false,
            Right = false,
            Down  = false,
            Left  = false,
            ID    = DataManager.dataManager.userData.playerData.id,
            X     = 0,
            Z     = 0
        };
        
        ConnectToServer(); //����������� � ������� � ������������� ���� �������
    }

    private void Update()
    {
        moveData.Up = false;
        moveData.Right = false;
        moveData.Down = false;
        moveData.Left = false;   

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveData.Up = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveData.Right = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveData.Down = true;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveData.Left = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            playerMenu.SetActive(true);
        }

        GameHendler(); //����� ���������� ����� ������� � ������������ �������
    }

    void FixedUpdate()
    {
        posText.text = $"x(-): {myPlayer.transform.position.x}\nz(|): {myPlayer.transform.position.z}";
        moveData.X = (float)Math.Round(myPlayer.transform.position.x);
        moveData.Z = (float)Math.Round(myPlayer.transform.position.z);

        if (moveData.Up || moveData.Right || moveData.Down || moveData.Left) // ���� ���� �������� ��
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
            print("----������: " + tcpClient.RemoteEndPoint);

            tcpGameHandler.Connect(SERVER_IP, SERVER_PORT);
            stream = tcpGameHandler.GetStream();
            print("----GameHandler ���������. ������: ");
        }
        catch
        {
            print("XXXXXX-������ �� ��������-XXXXXX");
            //��������� � ����
            ErrorIndicator.errorIndicator.ServerNotResponce();
            MenuManager.Instance.ShowMenu();
            SceneManager.LoadScene(0);
           
            return;
        }

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

        print("----��������� �� ������: " + JsonUtility.ToJson(toServerFirstCon));
        byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(toServerFirstCon));
        tcpClient.Send(data); // �������� ���� � ���� (id � ����������)

        data = new byte[4];
        tcpClient.Receive(data); // �������� ���-�� ������� � ����
        var playersAmount = int.Parse(Encoding.UTF8.GetString(data));
        tcpClient.Send(Encoding.UTF8.GetBytes("+")); // ���������� ������������� � ���������
        print($"----��������: ���������� ������� � ���� ({playersAmount})");

        for (int i = 0; i < playersAmount; ++i)
        {
            PlayerInfo playerInGameInfo = new() { };

            data = new byte[1024];
            var read_len = tcpClient.Receive(data); // �������� ���� � ��������� ������ � ����

            var player_data = new byte[read_len];
            Array.Copy(data, 0, player_data, 0, read_len);

            var playerInGameInfoJsonStr = Encoding.UTF8.GetString(player_data); // �� ���������� ������ � JSON
            print($"++++�������� JSON ������[{i}] - {playerInGameInfoJsonStr}");

            playerInGameInfo = JsonUtility.FromJson<PlayerInfo>(playerInGameInfoJsonStr); // �� JSON � ���������

            var playerPos = new Vector3(playerInGameInfo.X, 3.5f, playerInGameInfo.Z);
            var playerGO = Instantiate(playerPrefab, playerPos, Quaternion.identity); // ������� ������

            playersInGame.Add(playerInGameInfo.ID, playerGO); // ��������� ������ � ������

            tcpClient.Send(Encoding.UTF8.GetBytes("+")); // ���������� ������������� � ���������
            print("----����� ������� � ���������");
        }

        myPlayer = playersInGame[DataManager.dataManager.userData.playerData.id];
        myPlayer.GetComponent<MeshRenderer>().material = myColor; // �������� ���� ������ ������
    }

    void GameHendler()
    {
        if (stream.DataAvailable)
        {
            var data = new byte[1024];
            var read_len = stream.Read(data); ; // �������� ���� �� �������
            var server_data = new byte[read_len];
            Array.Copy(data, 0, server_data, 0, read_len);

            var info_from_server = Encoding.UTF8.GetString(server_data); // �� ���������� ������ � JSON
            print($"****�������� �� ������� - {info_from_server}");

            if (info_from_server == "servend")
            {
                print("------������ �������� ������-------");
                //��������� � ����
                ErrorIndicator.errorIndicator.ServerNotResponce();
                MenuManager.Instance.ShowMenu();
                SceneManager.LoadScene(0);
                
                return;
            }

            if (info_from_server[..2] == "np") //���� ����� �����
            {
                PlayerInfo new_player = JsonUtility.FromJson<PlayerInfo>(info_from_server[2..]); //���� � ����� ������

                var playerPos = new Vector3(new_player.X, 3.5f, new_player.Z);
                var playerGO = Instantiate(playerPrefab, playerPos, Quaternion.identity); // ������� ������

                playersInGame.Add(new_player.ID, playerGO); // ��������� ������ � ������
                print($"**����� ����� ���������: ID - {new_player.ID}; ����������: {playersInGame[new_player.ID].transform.position.x}; Z = {playersInGame[new_player.ID].transform.position.z}");
            }

            MoveDataFrom move_info = JsonUtility.FromJson<MoveDataFrom>(info_from_server); //���� �������� 

            Vector3 _movement = new();

            if (move_info.Up && move_info.Down)
            {
                _movement.z = 0;
            } 
            else if (move_info.Up)
            {
                _movement.z = 1;
            }
            else if (move_info.Down)
            {
                _movement.z = -1;
            } 
            else
            {
                _movement.z = 0;
            }

            if (move_info.Left && move_info.Right)
            {
                _movement.x = 0;
            }
            else if (move_info.Right)
            {
                _movement.x = 1;
            }
            else if (move_info.Left)
            {
                _movement.x = -1;
            }
            else
            {
                _movement.x = 0;
            }

            _movement.y = 0;

            playersInGame[move_info.ID].transform.Translate(speed * Time.deltaTime * _movement, Space.World); //����������� ������

        }
    }
}
