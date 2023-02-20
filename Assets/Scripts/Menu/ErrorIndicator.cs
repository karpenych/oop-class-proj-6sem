using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

public class ErrorIndicator : MonoBehaviour
{
    public static ErrorIndicator errorIndicator { get; private set; }
    public TMP_Text errorText;


    private void Awake()
    {
        errorIndicator = this;
    }

    public void DisplayError()
    {
        errorText.color = Color.red;
        errorText.text = DataManager.dataManager.userData.error.errorText;
    }

    public void DisplayRegister()
    {
        errorText.color = Color.green;
        errorText.text = "successful registration";
    }

    public void DisplayLogin()
    {
        errorText.color = Color.green;
        errorText.text = "login complete";
    }

    public void SetEmpty()
    {
        errorText.text = "";
    }
}
