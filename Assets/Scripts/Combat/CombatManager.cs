using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public Color aggroColor;
    [SerializeField] private Color movementColor = default;
    [SerializeField] private float timeBetweenTurns = default;
    [SerializeField] private float timeBetweenRounds = default;
    [SerializeField] private float timeAfterAttack = default;
    [HideInInspector] public List<GameObject> enemyList = new List<GameObject>();
    [HideInInspector] public List<GameObject> combatQueue = new List<GameObject>();
    [HideInInspector] public Unit currentUnit = null;

    [HideInInspector] public bool initiatingCombatState = false;
    private int currentIndex;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] public string whoseTurn = string.Empty;

    private void Awake()
    {
        instance = this;
        movementTilemap = GameObject.Find("MovementTilemap").GetComponent<Tilemap>();
    }

    // Adding all enemies to the enemyList. Later on the enemyList should contain only the enemies in the current room.
    private void Start()
    {
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemyList.Add(enemy);
        }
    }

    // Giving some time before combat to play animations and shit.
    public void InitiateCombat()
    {
        initiatingCombatState = true;
        StartCoroutine(WaitBetweenRounds(true));
    }

    // Getting all the units in the game (again, later on only in the current room) and queueing them based on the combat points.
    private void QueueUnits()
    {
        GameObject player = GameObject.Find("Player");
        combatQueue.Add(player);
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            combatQueue.Add(enemy);

        for(int i = 0; i < combatQueue.Count; ++i)
        {
            for(int j = i+1; j < combatQueue.Count; ++j)
            {
                if(combatQueue[i].GetComponent<Unit>().combatPoints < combatQueue[j].GetComponent<Unit>().combatPoints)
                {
                    var temp = combatQueue[i];
                    combatQueue[i] = combatQueue[j];
                    combatQueue[j] = temp;;
                }
            }
        }
    }

    public void ExecuteTurns()
    {
        if(currentUnit == null)
        {
            currentUnit = combatQueue[currentIndex].GetComponent<Unit>();
            currentUnit.currentCombatPoints = currentUnit.combatPoints;
        }
        else if(combatQueue[currentIndex].GetInstanceID() != currentUnit.gameObject.GetInstanceID())
        {
            currentUnit = combatQueue[currentIndex].GetComponent<Unit>();
            currentUnit.currentCombatPoints = currentUnit.combatPoints;
        }
        else
        {
            // Do nothing, as the turn didn't finish yet and we dont need to set the combat points.
        }
        if(whoseTurn == "Player")
        {
            Vector3Int playerPos = new Vector3Int((int)combatQueue[currentIndex].transform.position.x, (int)combatQueue[currentIndex].transform.position.y, 0);
            int speed = currentUnit.currentCombatPoints;
            MovementManager.instance.GenerateGrid(playerPos, speed, movementTilemap, movementColor);
        }
        else if(whoseTurn == "Skeleton")
        {
            currentUnit.transform.GetComponent<SkeletonAI>().BeginAI();
        }
    }

    // Increase our combat queue index and give some time so it won't be chaotic-looking.
    public void NextTurn()
    {
        ++currentIndex;
        if(currentIndex == combatQueue.Count)
        {
            currentIndex = 0;
            whoseTurn = combatQueue[currentIndex].name;
            StartCoroutine(WaitBetweenRounds());
        } 
        else
        {
            whoseTurn = combatQueue[currentIndex].name;
            StartCoroutine(WaitBetweenTurns());
        }
        
    }

    public void RemoveFromQueue(GameObject go)
    {
        for(int i = 0; i < combatQueue.Count; ++i)
        {
            if(combatQueue[i].GetInstanceID() == go.GetInstanceID())
            {
                combatQueue.RemoveAt(i);
                break;
            }
        }

        if(combatQueue.Count <= 1)
        {
            EndCombat();
            enemyList.Clear();
        }
    }

    private void EndCombat()
    {
        GameStateManager.instance.previousGameState = GameStateManager.instance.gameState;
        GameStateManager.instance.gameState = GameStateManager.GameStates.EXPLORING;
        combatQueue.Clear();
        UIManager.instance.EndQueueUI();
    }

    private IEnumerator WaitBetweenTurns()
    {
        float timer = 0f;
        while(timer < timeBetweenTurns)
        {
            timer += Time.deltaTime;
            if(timer >= timeBetweenTurns)
            {
                ExecuteTurns();
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    private IEnumerator WaitBetweenRounds()
    {
        float timer = 0f;
        while(timer < timeBetweenRounds)
        {
            timer += Time.deltaTime;
            if(timer >= timeBetweenRounds)
            {
                foreach(GameObject go in combatQueue)
                {
                    Unit goUnit = go.GetComponent<Unit>();
                    goUnit.currentCombatPoints = goUnit.combatPoints;
                }

                if(GameStateManager.instance.gameState != GameStateManager.GameStates.COMBAT)
                    yield break;

                ExecuteTurns();
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    // Overloading for the first time the combat starts since we have to set up additional things.
    private IEnumerator WaitBetweenRounds(bool combatStarting)
    {
        float timer = 0f;
        while(timer < timeBetweenRounds)
        {
            timer += Time.deltaTime*2; // Not sure why I have to double
            if(timer >= timeBetweenRounds)
            {
                initiatingCombatState = false;

                QueueUnits();
                UIManager.instance.InitiateQueueUI(combatQueue);
                
                currentIndex = 0;
                whoseTurn = combatQueue[currentIndex].name;
                ExecuteTurns();
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    public IEnumerator WaitAfterAttack(Unit unit)
    {
        float timer = 0f;
        while(timer < timeAfterAttack)
        {
            timer += Time.deltaTime;
            if(timer >= timeAfterAttack)
            {
                // If not already in combat, enter it (for something like sneak attacks or distance attacks).
                if(GameStateManager.instance.gameState == GameStateManager.GameStates.MOVING)
                {
                    if(combatQueue.Count != 0)
                    {
                        GameStateManager.instance.previousGameState = GameStateManager.instance.gameState;
                        GameStateManager.instance.gameState = GameStateManager.GameStates.COMBAT;

                        foreach(GameObject enemy in CombatManager.instance.enemyList)
                            enemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

                        unit.currentCombatPoints = unit.combatPoints;
                        InitiateCombat();
                    }
                    
                }
                else if(GameStateManager.instance.gameState != GameStateManager.GameStates.COMBAT)
                {
                    yield break;
                }
                else if(unit.currentCombatPoints <= 0)
                {
                    NextTurn();
                }
                else
                {
                    ExecuteTurns();
                }
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    public int GetObjectIndex(GameObject go)
    {
        if(combatQueue.Count != 0)
            for(int i = 0; i < combatQueue.Count; ++i)
                if(combatQueue[i].GetInstanceID() == go.GetInstanceID())
                    return i;
        return 0;
    }

}