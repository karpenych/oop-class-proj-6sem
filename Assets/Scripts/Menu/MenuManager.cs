using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button _btStart;
    [SerializeField] GameObject registerField;
    [SerializeField] GameObject menu;
    public static MenuManager Instance { get; private set; }


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

    private void Start()
    {
        _btStart.enabled = false;
    }

    public void OnQuitClick() // QUIT
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void CLoseRegisterClick() // REGISTER FIELD OFF
    {
        registerField.SetActive(false);
    }

    
    public void OnRegisterClick() // REGISTER FIELD ON
    { 
        registerField.SetActive(true);
    }

    public void OnStartClick() // START
    {
        menu.SetActive(false);
        SceneManager.LoadScene("Main");   
    }

    public void ShowMenu()
    {
        menu.SetActive(true);
    }
}
