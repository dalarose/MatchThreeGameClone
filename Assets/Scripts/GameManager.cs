using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float _startingTime = 60f;

    [SerializeField]
    private UIManager _UIManager;

    [SerializeField]
    private ShapesManager _shapesManager;

    [SerializeField]
    private MainMenu _mainMenu;

    private float _currentTime = 0;

    private const string _savedLevel = "savedLevel";
    private const string _savedTime = "savedTime";
    private const string _savedScore = "savedScore";

    public int CurrentLevel;

    void Start()
    {
        _currentTime = _startingTime;

        Load();

        if (CurrentLevel == 0)
        {
            CurrentLevel = 1;
            _currentTime = _startingTime;
            _UIManager.LevelText(CurrentLevel);
        }
    }

    void Update()
    {
        CountdownTimer();

        if (Input.GetKeyDown(KeyCode.P))
        {
            print(_shapesManager.Score);
            print(CurrentLevel);
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }

        else if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
    }

    private void CountdownTimer()
    {
        _currentTime -= 1 * Time.deltaTime;
        _UIManager.CountdownTimerText(_currentTime);

        if (_currentTime <= 0)
        {
            _currentTime = 0;
            LoseState();
        }
    }

    private void LoseState()
    {
        _UIManager.PlayerLoseSequence();
        Time.timeScale = 0;
    }

    public void WinState()
    {        
        _UIManager.PlayerWinsSequence();
        Time.timeScale = 0;
    }
    private void LevelUpdate()
    {
        Time.timeScale = 1;
        _currentTime = _startingTime;
        _shapesManager.InitializeTypesOnPrefabShapesAndBonuses();
        _shapesManager.InitializeCandyAndSpawnPositions();
        _shapesManager.StartCheckForPotentialMatches();
       // _shapesManager.InitializeVariables();
    }

    private void NextLevel()
    {
        CurrentLevel ++;
        _UIManager.LevelText(CurrentLevel);
        _UIManager.NextLevelUIReset();
        LevelUpdate();
    }

    private void RestartLevel()
    {
        _UIManager.RestartLevelUIReset();
        LevelUpdate();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(_savedLevel, CurrentLevel);
        PlayerPrefs.SetFloat(_savedTime, _currentTime);
        PlayerPrefs.SetInt(_savedScore, _shapesManager.Score);
    }

    private void Load()
    {
        CurrentLevel = PlayerPrefs.GetInt(_savedLevel,1);
        _currentTime = PlayerPrefs.GetFloat(_savedTime,60f);
        _shapesManager.Score = PlayerPrefs.GetInt(_savedScore,0);
        _UIManager.LevelText(CurrentLevel);
        _shapesManager.ShowScore();
    }

    public void ExitWithSave()
    {
        Save();
        Time.timeScale = 1;
        _mainMenu.LoadMainMenu();
    }

    public void ExitWithoutSave()
    {
        Time.timeScale = 1;
        _mainMenu.LoadMainMenu();
    }
}
