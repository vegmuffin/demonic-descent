using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public bool isEnemy;
    public int health;
    public int combatPoints;
    public int damage;
    [SerializeField] private int aggroRange = default;
    [SerializeField] private float deathTime = default;

    private Color aggroColor;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] public int currentCombatPoints;
    [HideInInspector] public int hoveringCombatPoints;

    public bool isDying = false;

    private void Awake()
    {
        currentCombatPoints = combatPoints;
    }

    private void Start()
    {
        if(isEnemy)
        {
            movementTilemap = transform.parent.GetChild(1).GetChild(0).GetComponent<Tilemap>();
            aggroColor = CombatManager.instance.aggroColor;
        }
        
    }

    // Start is called before the first frame update
    private void Update()
    {
        if(isEnemy)
            if(Input.GetKeyDown(KeyCode.A))
            {
                AggroGrid();
            }
    }

    // Painting the aggro range grid. If a player steps on the grid, combat will be initiated.
    private void AggroGrid()
    {
        Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        MovementManager.instance.GenerateGrid(pos, combatPoints, movementTilemap, aggroColor);
    }

    private void OnMouseOver()
    {
        if(isEnemy)
        {
            if(CursorManager.instance.currentState != CursorManager.CursorStates.ATTACK)
            {
                if(isDying)
                    CursorManager.instance.currentState = CursorManager.CursorStates.DEFAULT;
                else
                    CursorManager.instance.currentState = CursorManager.CursorStates.ATTACK;
            }
        }
    }

    private void OnMouseExit()
    {
        CursorManager.instance.currentState = CursorManager.CursorStates.DEFAULT;
    }

    public void OnDamage()
    {
        if(health <= 0)
        {
            // Play some animation
            isDying = true;
            Destroy(transform.parent.gameObject, deathTime);
            MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), true);
        }
        else
        {
            // Play some animation
        }
    }

    public bool CanAct()
    {
        if(hoveringCombatPoints <= currentCombatPoints)
            return true;
        return false;
    }
}
