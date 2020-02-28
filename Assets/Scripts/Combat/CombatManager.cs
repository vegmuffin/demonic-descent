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

    [HideInInspector] public bool initiatingCombatState = false;
    private int currentIndex;
    [HideInInspector] public Tilemap movementTilemap;
    private float waitTurnsTimer;
    private float waitRoundsTimer;
    private float afterAttackTimer;
    private bool afterAttackBool = false;
    [HideInInspector] public string whoseTurn = string.Empty;

    private void Awake()
    {
        instance = this;
        waitTurnsTimer = timeBetweenTurns;
        waitRoundsTimer = timeBetweenRounds;
        afterAttackTimer = 0;
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
        Unit currentUnit = combatQueue[currentIndex].GetComponent<Unit>();
        Debug.Log(currentUnit);
        if(whoseTurn == "Player")
        {
            Vector3Int playerPos = new Vector3Int((int)combatQueue[currentIndex].transform.position.x, (int)combatQueue[currentIndex].transform.position.y, 0);
            int speed = currentUnit.currentCombatPoints;
            Debug.Log("Current combat points: " + speed);
            MovementManager.instance.GenerateGrid(playerPos, speed, movementTilemap, movementColor);
        }
        else if(whoseTurn == "Skeleton")
        {
            Debug.Log("Time to execute other turns");
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
        }
        whoseTurn = combatQueue[currentIndex].name;
        StartCoroutine(WaitBetweenTurns());
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
    }

    private IEnumerator WaitBetweenTurns()
    {
        while(waitTurnsTimer > 0)
        {
            waitTurnsTimer -= Time.deltaTime;
            if(waitTurnsTimer <= 0)
            {
                waitTurnsTimer = timeBetweenTurns;
                ExecuteTurns();
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    private IEnumerator WaitBetweenRounds()
    {
        while(waitRoundsTimer > 0)
        {
            waitRoundsTimer -= Time.deltaTime;
            if(waitRoundsTimer <= 0)
            {
                waitRoundsTimer = timeBetweenTurns;
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
    }

    // Overloading for the first time the combat starts since we have to set up additional things.
    private IEnumerator WaitBetweenRounds(bool combatStarting)
    {
        while(waitRoundsTimer > 0)
        {
            waitRoundsTimer -= Time.deltaTime*2; // Not sure why I have to double
            if(waitRoundsTimer <= 0)
            {
                initiatingCombatState = false;
                waitRoundsTimer = timeBetweenTurns;
                QueueUnits();
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
        while(afterAttackTimer < timeAfterAttack)
        {
            afterAttackTimer += Time.deltaTime;
            if(afterAttackTimer >= timeAfterAttack)
            {
                afterAttackTimer = 0;
                // If not already in combat, enter it (for something like sneak attacks or distance attacks).
                if(GameStateManager.instance.gameState != GameStateManager.GameStates.COMBAT)
                {
                    GameStateManager.instance.previousGameState = GameStateManager.instance.gameState;
                    GameStateManager.instance.gameState = GameStateManager.GameStates.COMBAT;

                    foreach(GameObject enemy in CombatManager.instance.enemyList)
                        enemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

                    unit.currentCombatPoints = unit.combatPoints;
                    InitiateCombat();
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

}