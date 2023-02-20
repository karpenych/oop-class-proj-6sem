using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using UnityEngine.EventSystems;


[System.Serializable]
public class UserData
{
    public Player playerData;
    public Error error;
}

[System.Serializable]
public class Error
{
    public string errorText;
    public bool isError;
}

[System.Serializable]
public class Player
{
    public int id;
    public string pos;
}




public class DataManager : MonoBehaviour
{
    public static DataManager dataManager { get; private set; }
    public UserData userData;
    [SerializeField] Vector3 _firstSpawnPos;


    private void Awake()
    {
        if (dataManager != null)
        {
            Destroy(gameObject);
            return;
        }

        dataManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _firstSpawnPos = new(0, 0, 0);
    }

    public void SaveData()
    {
        WebManager.Instance.SaveData(userData.playerData.id, userData.playerData.pos);
    }

    public Vector3 LoadSpawnPosition()
    {
        if (userData.playerData.pos != "Null")
        {
            return SetPosition(JsonUtility.FromJson<Vector3>(userData.playerData.pos));
        }
        else
        {
            return SetPosition(_firstSpawnPos);
        }
    }

    public Vector3 SetPosition(Vector3 pos)
    {
        var spawn_pos = pos;
        spawn_pos.y += 3;
        return spawn_pos;
    }

}
