using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public bool isEnemy;
    [SerializeField] private int health;
    public int combatPoints;
    [SerializeField] private int damage;
    [SerializeField] private int aggroRange;

    private Color aggroColor;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] int currentCombatPoints;

    private void Start()
    {
        if(isEnemy)
        {
            movementTilemap = transform.GetChild(0).GetChild(0).GetComponent<Tilemap>();
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

    private void AggroGrid()
    {
        Vector3Int pos = Vector3Int.zero;
        MovementManager.instance.GenerateGrid(pos, combatPoints, movementTilemap, aggroColor);
    }
}
