using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UserData
{
    //Player playerData;
    public Error error;
}

[System.Serializable]
public class Error
{
    public string errorText;
    public bool isError;
}

/*[System.Serializable]
public class Player
{
    public Vector3 pos;
}*/




public class DataManager : MonoBehaviour
{
    public static DataManager dataManager { get; private set; }
    public UserData userData;
    [SerializeField] WebManager webManager;


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

    public void LoadData()
    {

    }

    public void SaveData()
    {
        webManager.SaveData();
    }



}
