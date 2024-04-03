using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameManager GM;

    [SerializeField] private MenuElements _menu;
    [SerializeField] private HUDElements _hud;

    public MenuElements Menu
    {
        get { return _menu; }
    }
        
    public HUDElements HUD
    {
        get { return _hud; }
        set { _hud = value; }
    }  

    void Update()
    {
        _menu.TimeScaleText.text = Time.timeScale.ToString();
    }

    // Scores functions
    public void GameOverButton()
    {
        GM.GameOver(true);
    }

    // HUD Function definitons

    public void UpdateTimeText(int time)
    {
        _hud.TimerText.text = "Timer: ";
        if (time < 10)
        {
            _hud.TimerText.text += "00" + time;
        }
        else if (time < 100)
        {
            _hud.TimerText.text += "0" + time;
        }
        else if (time < 1000)
        {
            _hud.TimerText.text += time.ToString();
        }
    }

    public void UpdateFlagText(int flagCount)
    {
        _hud.FlagText.text = "Flags: ";

        // handle the sign of the counter
        string flagCountText = "";
        if (flagCount < 0)
        {
            flagCountText += "-";
        }

        // set the counter value according to digit number of flag count
        flagCount = Mathf.Abs(flagCount);   // ignore sign
        if (Mathf.Abs(flagCount) < 10)
        {
            flagCountText += "00";
        }
        else if (Mathf.Abs(flagCount) < 100)
        {
            flagCountText += "0";
        }
        flagCountText += flagCount;
        

        // add the constructed flag count text to the UI Text
        _hud.FlagText.text += flagCountText;

    }

    public void RestartButton()
    {
        GM.StartNewGame(GM.Settings);
    }

    public void ResetHUD(int flagCount)
    {
        HUD.GameStateText.enabled = false;
        UpdateFlagText(flagCount);
        UpdateTimeText(0);   
    }
}

[Serializable]
public class MenuElements
{

    // Debug UI variables
    [SerializeField] private Text _timeScaleText;

    // getters & setters
    public Text TimeScaleText
    {
        get { return _timeScaleText; }
        set { _timeScaleText = value; }
    }

}

[Serializable]
public class HUDElements
{
    [SerializeField] private Text _timerText;
    [SerializeField] private Text _flagText;
    [SerializeField] private Text _gameStateText;  // won/lost


    // getters & setters
    public Text TimerText
    {
        get { return _timerText; }
        set { _timerText = value; }
    }

    public Text FlagText
    {
        get { return _flagText; }
        set { _flagText = value; }
    }

    public Text GameStateText
    {
        get { return _gameStateText; }
        set { _gameStateText = value; }
    }
}


