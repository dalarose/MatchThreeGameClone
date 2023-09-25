using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Text _winText;

    [SerializeField]
    private Text _loseText;

    [SerializeField]
    private Text _levelText;

    [SerializeField]
    private Text _timerText;

    [SerializeField]
    private Button _nextLevelButton;

    [SerializeField]
    private Button _restartLevelButton;

    [SerializeField]
    private Button _restartButton;

    [SerializeField]
    private Button _premadeLevelButton;

    [SerializeField]
    private Image _optionsMenuBackground;

    [SerializeField]
    private ShapesManager _shapesManager;

    [SerializeField]
    private GameManager _gameManager;

    [SerializeField]
    private Text _startLevelText;

    private bool _isOptionsMenuOpen = false;

    void Start()
    {
        _startLevelText.text = "Level: " + _gameManager.CurrentLevel;
        _winText.gameObject.SetActive(false);
        _loseText.gameObject.SetActive(false);
        _nextLevelButton.gameObject.SetActive(false);
        _restartLevelButton.gameObject.SetActive(false);
        _restartButton.gameObject.SetActive(false);
        _premadeLevelButton.gameObject.SetActive(false);
        _optionsMenuBackground.gameObject.SetActive(false);
    }

    public void PlayerWinsSequence()
    {
        _winText.gameObject.SetActive(true);
        _nextLevelButton.gameObject.SetActive(true);
    }

    public void PlayerLoseSequence()
    {
        _loseText.gameObject.SetActive(true);
        _restartLevelButton.gameObject.SetActive(true);
    }

    public void NextLevelUIReset()
    {
        _winText.gameObject.SetActive(false);
        _nextLevelButton.gameObject.SetActive(false);
    }

    public void RestartLevelUIReset()
    {
        _loseText.gameObject.SetActive(false);
        _restartLevelButton.gameObject.SetActive(false);
    }

    public void CountdownTimerText(float currentTime)
    {
        _timerText.text = "Time Remaining: " + currentTime.ToString("0");
    }

    public void LevelText(int currentLevel)
    {
        _levelText.text = "Level: " + currentLevel.ToString();
    }

    public void OptionsMenuButton()
    {
        if (!_isOptionsMenuOpen)
        {            
            ShowOptionsMenu(true);
        }
        else if (_isOptionsMenuOpen)
        {
            ShowOptionsMenu(false);
        }        
    }

    private void ShowOptionsMenu(bool active)
    {
        _restartButton.gameObject.SetActive(active);
        _premadeLevelButton.gameObject.SetActive(active);
        _optionsMenuBackground.gameObject.SetActive(active);
        _isOptionsMenuOpen = active;
        Time.timeScale = active ? 0 : 1;
    }

    public void RestartButton()
    {
        ShowOptionsMenu(false);
        _shapesManager.InitializeCandyAndSpawnPositions();
    }

    public void PremadeLevelButton()
    {
        ShowOptionsMenu(false);      
        _shapesManager.InitializeCandyAndSpawnPositionsFromPremadeLevel();
    }
}
