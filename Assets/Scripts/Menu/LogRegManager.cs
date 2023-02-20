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


    public void OnSignInClick() // SIGN IN
    {
        WebManager.Instance.Login(signIn.siLogin.text, signIn.siPassword.text);
        print("SignIn Clicked");
    }

    public void OnRegisterClick() // REGISTER
    {
        WebManager.Instance.Registration(reg.regLogin.text, reg.regPassword.text, reg.regConfirmPassword.text);
    }
}
