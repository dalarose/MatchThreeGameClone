using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const int _titleMenu = 0;
    private const int _mainGame = 1;
    private const int _settingsMenu = 2;

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(_titleMenu);
    }

    public void ContinueMainGame()
    {
        SceneManager.LoadScene(_mainGame);
    }

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(_mainGame);
    }

    public void LoadSettingsMenu()
    {
        SceneManager.LoadScene(_settingsMenu);
    }
}
