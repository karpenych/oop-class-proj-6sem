using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogRegManager : MonoBehaviour
{
    [System.Serializable]
    public class MenuSignIn
    {
        public TMP_Text siLogin, siPassword;
    }

    [System.Serializable]
    public class MenuRegistration
    {
        public TMP_Text regLogin, regPassword, regConfirmPassword;
    }

    public MenuSignIn signIn;
    public MenuRegistration reg;

    [SerializeField] Button _btStart;
    [SerializeField] WebManager webManager;
    


    public void OnSignInClick() // SIGN IN
    {
        webManager.Login(signIn.siLogin.text, signIn.siPassword.text);
    }

    public void OnRegisterClick() // REGISTER
    {
        webManager.Registration(reg.regLogin.text, reg.regPassword.text, reg.regConfirmPassword.text);
    }
}
