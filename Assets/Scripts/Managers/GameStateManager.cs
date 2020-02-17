using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    
    public enum GameStates
    {
        EXPLORING,
        COMBAT,
        PAUSE,
        MOVING
    }

    public GameStates gameState;
    public GameStates previousGameState;

    private void Awake()
    {
        instance = this;
        gameState = GameStates.EXPLORING;
    }
}
