using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingUI;
    public GameObject creditUI;

    void Awake()
    {
        settingUI.SetActive(false);
        creditUI.SetActive(false);
    }
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Setting()
    {
        settingUI.SetActive(true);
    }
    public void ExitSetting()
    {
        settingUI.SetActive(false);
    }
    public void Credit()
    {
        creditUI.SetActive(true);
    }
    public void ExitCredit()
    {
        creditUI.SetActive(false);
    }
}
