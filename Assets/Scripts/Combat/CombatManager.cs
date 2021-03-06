using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public Color aggroColor;
    [SerializeField] private Color movementColor = default;
    [SerializeField] private float timeBetweenRounds = default;
    [SerializeField] private float timeAfterAttack = default;
    public AnimationCurve cameraShakeSpeedCurve;
    public float flashSpriteRate;
    [Space]
    [SerializeField] private GameObject block = default;

    [HideInInspector] public List<GameObject> enemyList = new List<GameObject>();
    [HideInInspector] public List<GameObject> combatQueue = new List<GameObject>();
    [HideInInspector] public List<GameObject> combatOrder = new List<GameObject>();
    [HideInInspector] public Dictionary<GameObject, bool> unitWaits = new Dictionary<GameObject, bool>();
    [HideInInspector] public Unit currentUnit = null;

    [HideInInspector] public bool initiatingCombatState = false;
    private int currentIndex;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] public string whoseTurn = string.Empty;
    [HideInInspector] public int currentRound = 0;

    private List<GameObject> blockages = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        movementTilemap = GameObject.Find("MovementTilemap").GetComponent<Tilemap>();
    }

    // Adding all enemies to the enemyList. Later on the enemyList should contain only the enemies in the current room.
    private void Start()
    {
        Resources.LoadAll("");
        enemyList.Clear();
    }

    // Giving some time before combat to play animations and shit.
    public void InitiateCombat()
    {
        // Combat has been engaged, setting states, clearing tiles.
        GameStateManager.instance.ChangeState("COMBAT");

        foreach(GameObject anotherEnemy in CombatManager.instance.enemyList)
            anotherEnemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

        // Clearing previous combat queue
        UIManager.instance.ClearCombatQueue();

        initiatingCombatState = true;
        BlockOff();
        StartCoroutine(WaitBetweenRounds(true));

        /* // Creating a log entry.
        string logEntry = "Battle is starting!";
        UILog.instance.NewLogEntry(logEntry); */
    }

    // When the battle begins, block of the room to prevent from engaging in other encounters.
    private void BlockOff()
    {
        Room currentRoom = RoomManager.instance.currentRoom;
        List<Vector3Int> blockCoords = currentRoom.blockingCoords;

        foreach(Vector3Int coord in blockCoords)
        {
            GameObject blockage = Instantiate(block, coord, Quaternion.identity);
            blockages.Add(blockage);
            MovementManager.instance.UpdateTileWalkability(coord, false, null);
        }
    }

    // Clear the blockages when the battle has ended.
    private void ClearBlockages()
    {
        foreach(GameObject blockage in blockages)
        {
            Vector3 trPos = blockage.transform.position;
            Vector3Int position = new Vector3Int((int)trPos.x, (int)trPos.y, 0);
            MovementManager.instance.UpdateTileWalkability(position, true, null);
            Destroy(blockage);
        }
        
        blockages.Clear();
        
    }

    // Getting all the units in the game and queueing them based on the combat points.
    private void QueueUnits()
    {
        GameObject player = GameObject.Find("Player");
        var playerUnit = player.GetComponent<Unit>();
        playerUnit.currentCombatPoints = playerUnit.combatPoints;

        combatQueue.Add(player);
        foreach(GameObject enemy in enemyList)
        {
            combatQueue.Add(enemy);
            var enemyUnit = enemy.GetComponent<Unit>();
            enemyUnit.currentCombatPoints = enemyUnit.combatPoints;
        }

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

        for(int i = 0; i < combatQueue.Count; ++i)
        {
            combatOrder.Add(combatQueue[i]);
            unitWaits.Add(combatQueue[i], false);
        }
    }

    public void ExecuteTurns()
    {

        if(currentUnit == null)
            currentUnit = combatQueue[currentIndex].GetComponent<Unit>();
        else if(combatQueue[currentIndex].GetInstanceID() != currentUnit.gameObject.GetInstanceID())
            currentUnit = combatQueue[currentIndex].GetComponent<Unit>();
        else
        {
            // Do nothing, as the turn didn't finish yet and we dont need to set the combat points.
        }
        whoseTurn = combatQueue[currentIndex].name;
        if(whoseTurn == "Player")
        {
            Vector3Int playerPos = new Vector3Int((int)combatQueue[currentIndex].transform.position.x, (int)combatQueue[currentIndex].transform.position.y, 0);
            int speed = currentUnit.currentCombatPoints;
            MovementManager.instance.GenerateAttackGrid(playerPos, speed, movementTilemap, movementColor);
            UIManager.instance.skipButton.interactable = true;
            if(!unitWaits[combatQueue[currentIndex]])
                UIManager.instance.waitButton.interactable = true;
        }
        else if(whoseTurn == "Skeleton")
        {
            currentUnit.transform.GetComponent<SkeletonAI>().BeginAI();
        }
    }

    // Increase our combat queue index and give some time so it won't be chaotic-looking.
    public void NextTurn()
    {
        if(combatQueue[currentIndex].tag == "Player")
        {
            UIManager.instance.waitButton.interactable = false;
            UIManager.instance.skipButton.interactable = false;
        }

        UIManager.instance.updatingCombatPoints = null;
        ++currentIndex;
        if(currentIndex == combatQueue.Count)
        {
            currentIndex = 0;

            // It's not possible to just copy over by assignment, since collections are reference type variables, it will lead to the same object.
            combatQueue.Clear();
            for (int i = 0; i < combatOrder.Count; ++i)
                combatQueue.Add(combatOrder[i]);

            currentUnit = null;
            whoseTurn = "";

            foreach(GameObject go in combatOrder)
            {
                var unit = go.GetComponent<Unit>();
                unit.currentCombatPoints = unit.combatPoints;
                unitWaits[go] = false;
            }

            UIManager.instance.NextRound();
        } 
        else
        {
            whoseTurn = combatQueue[currentIndex].name;
            UIManager.instance.HideElement();
        }
        
    }

    public void AfterWait(GameObject go)
    {
        int index = GetObjectIndex(go);
        combatQueue.RemoveAt(index);
        combatQueue.Add(go);
        whoseTurn = combatQueue[currentIndex].name;
        unitWaits[go] = true;
        UIManager.instance.PushDownElement(index+1);
        ExecuteTurns();
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

        for(int i = 0; i < combatOrder.Count; ++i)
        {
            if(combatOrder[i].GetInstanceID() == go.GetInstanceID())
            {
                combatOrder.RemoveAt(i);
                break;
            }
        }

        if(combatOrder.Count <= 1)
            EndCombat();
    }

    private void EndCombat()
    {
        GameStateManager.instance.ChangeState("EXPLORING");
        enemyList.Clear();
        combatQueue.Clear();
        combatOrder.Clear();
        RoomManager.instance.currentRoom.enemyList.Clear();
        UIManager.instance.EndQueueUI();
        UIManager.instance.ResetCombatPointsBar();
        ClearBlockages();

        /* // Creating a log entry.
        string logEntry = "Battle ends with a victory!";
        UILog.instance.NewLogEntry(logEntry); */

        currentRound = 0;
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
                CursorManager.instance.combatPointsIndicator.gameObject.SetActive(true);
                ++currentRound;
                ExecuteTurns();
                yield break;
            }
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
        yield break;
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
                if(GameStateManager.instance.CheckState("MOVING"))
                {
                    if(combatQueue.Count != 0)
                    {
                        GameStateManager.instance.ChangeState("COMBAT");

                        foreach(GameObject enemy in CombatManager.instance.enemyList)
                            enemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

                        unit.currentCombatPoints = unit.combatPoints;
                        InitiateCombat();
                    }
                    
                }
                else if(!GameStateManager.instance.CheckState("COMBAT"))
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

        yield break;
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