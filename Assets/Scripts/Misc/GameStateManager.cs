using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    private Image currentStateImage;
    private Image previousStateImage;

    [SerializeField] private Sprite exploringImage = default;
    [SerializeField] private Sprite combatImage = default;
    [SerializeField] private Sprite snoozeImage = default;
    [SerializeField] private Sprite movingImage = default;
    
    public enum GameStates
    {
        EXPLORING,
        COMBAT,
        SNOOZE,
        MOVING
    }

    public GameStates gameState;
    public GameStates previousGameState;

    private void Awake()
    {
        instance = this;
        gameState = GameStates.EXPLORING;

        currentStateImage = GameObject.Find("GameStateIndicator").GetComponent<Image>();
        previousStateImage = currentStateImage.transform.Find("Previous").GetComponent<Image>();

        // This alleviates 'SOME' performance issues
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    public void ChangeState(string state)
    {
        previousGameState = gameState;
        previousStateImage.sprite = currentStateImage.sprite;
        
        if(state == "EXPLORING")
        {
            currentStateImage.sprite = exploringImage;
            gameState = GameStates.EXPLORING;
        }
        else if(state == "COMBAT")
        {
            currentStateImage.sprite = combatImage;
            gameState = GameStates.COMBAT;
        }
        else if(state == "SNOOZE")
        {
            currentStateImage.sprite = snoozeImage;
            gameState = GameStates.SNOOZE;
        }
        else if(state == "MOVING")
        {
            currentStateImage.sprite = movingImage;
            gameState = GameStates.MOVING;
        }
        else
        {
            Debug.LogError("Unknown game state specified when trying to change state.");
        }
    }

    public bool CheckState(string state)
    {
        if(state == "EXPLORING")
            if(gameState != GameStates.EXPLORING)
                return false;
            else
                return true;

        else if(state == "COMBAT")
            if(gameState != GameStates.COMBAT)
                return false;
            else
                return true;

        else if(state == "SNOOZE")
            if(gameState != GameStates.SNOOZE)
                return false;
            else
                return true;

        else if(state == "MOVING")
            if(gameState != GameStates.MOVING)
                return false;
            else
                return true;

        else
        {
            Debug.LogError("Unknown game state specified when checking for a game state.");
            return false;
        }
    }
}
