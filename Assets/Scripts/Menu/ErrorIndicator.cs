using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;

public class ErrorIndicator : MonoBehaviour
{
    public static ErrorIndicator errorIndicator;
    public TMP_Text errorText;



    private void Awake()
    {
        errorIndicator = this;
    }

    public void DisplayError()
    {
        errorText.text = DataManager.dataManager.userData.error.errorText;
    }
}
