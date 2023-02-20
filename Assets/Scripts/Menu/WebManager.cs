using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebManager : MonoBehaviour
{
    [SerializeField] GameObject registerField;
    [SerializeField] Button _btStart;
    [SerializeField] string targetURL;
    [SerializeField] UnityEvent OnError, OnRegister, OnLogin;
    public static WebManager Instance { get; private set; } 

    public enum RequestType
    {
        logging, register, save
    }



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // OnClick functions

    public void Login(string login, string password) // LOGIN
    {
        StopAllCoroutines();
        if (CheckString(login) && CheckString(password))
        {
            Logging(login, password);
        }
        else
        {
            DataManager.dataManager.userData.error.errorText = "To small length";
            OnError.Invoke();
        }
    }

    public void Registration(string login, string password, string confirmPassword) // REGISTER
    {
        StopAllCoroutines(); 
        if (CheckString(login) && CheckString(password) && CheckString(confirmPassword))
        {
            Registering(login, password, confirmPassword);
        }
        else
        {
            DataManager.dataManager.userData.error.errorText = "To small length";
            OnError.Invoke();
        }
        
    }

    public void SaveData(int id, string pos) // DATA SAVING
    {
        StopAllCoroutines();
        SavingData(id, pos);
    }

    public bool CheckString(string toCheck)
    {
        toCheck = toCheck.Trim();
        if (toCheck.Length > 4 && toCheck.Length < 16)
        {
            return true;
        }
        return false;
    }

    // Web functions

    public UserData SetUserData(string data)
    {
        print(data);
        return JsonUtility.FromJson<UserData>(data);
    }

    void Logging(string login, string password)
    {
        var form = new WWWForm();
        form.AddField("type", RequestType.logging.ToString());
        form.AddField("login", login);
        form.AddField("password", password);
        StartCoroutine(SendData(form, RequestType.logging));
    }

    void Registering(string login, string password, string confirmPassword)
    {
        var form = new WWWForm();
        form.AddField("type", RequestType.register.ToString());
        form.AddField("login", login);
        form.AddField("password", password);
        form.AddField("confirmPassword", confirmPassword);
        StartCoroutine(SendData(form, RequestType.register));
    }

    public void SavingData(int id, string pos)
    {
        var form = new WWWForm();
        form.AddField("type", RequestType.save.ToString());
        form.AddField("id", id);
        form.AddField("pos", pos);
        StartCoroutine(SendData(form, RequestType.save));
    }

    IEnumerator SendData(WWWForm form, RequestType type)
    {
        using UnityWebRequest www = UnityWebRequest.Post(targetURL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var data = SetUserData(www.downloadHandler.text);
            if (!data.error.isError)
            {
                if (type != RequestType.save)
                {
                    DataManager.dataManager.userData = data;
                    if (type == RequestType.logging)
                    {
                        _btStart.enabled = true;
                        OnLogin.Invoke();
                    }
                    else
                    {
                        registerField.SetActive(false);
                        OnRegister.Invoke();
                    }
                }
            }
            else
            {
                DataManager.dataManager.userData = data;
                OnError.Invoke();
            }
        }
    }



}


