﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public Sprite combatQueueImage;
    public bool isEnemy;
    public int health;
    public int combatPoints;
    public int damage;
    public string acronym;
    [SerializeField] private int aggroRange = default;
    [SerializeField] private float deathTime = default;

    private Color aggroColor;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] public int currentCombatPoints;
    [HideInInspector] public int hoveringCombatPoints;

    [HideInInspector] public bool isDying = false;
    private Animator thisAnimator;
    private string lookDir = "Front";

    private void Awake()
    {
        currentCombatPoints = combatPoints;
        if(!isEnemy)
            thisAnimator = transform.GetComponent<Animator>();
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
            int index = CombatManager.instance.GetObjectIndex(gameObject);
            UIManager.instance.DeathChange(index);

            var box = transform.GetComponent<BoxCollider2D>();
            if(CursorManager.instance.currentState == CursorManager.CursorStates.ATTACK && box.bounds.Contains((Vector2)CursorManager.instance.transform.position))
            {
                OnMouseExit();
            }
            // Play some animation
            isDying = true;
            MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), true);
            CombatManager.instance.RemoveFromQueue(transform.gameObject);
            Destroy(transform.parent.gameObject, deathTime);

            if(gameObject.name == "Player")
            {
                Debug.Log("Game over!");
            }
        }
        else
        {
            if(GameStateManager.instance.gameState == GameStateManager.GameStates.COMBAT)
            {   
                int index = CombatManager.instance.GetObjectIndex(gameObject);
                UIManager.instance.HealthChange(index, health);
            }
            
            // Play some animation
        }
    }

    public bool CanAct()
    {
        if(hoveringCombatPoints <= currentCombatPoints)
            return true;
        return false;
    }

    public void PlayAnimation(Vector2 dir, string animationName, float speed)
    {
        thisAnimator.SetFloat("animSpeed", speed);
        string name = animationName + "Animation";
        if(dir == Vector2.left)
        {
            name += "Left";
            lookDir = "Left";
        }
        else if(dir == Vector2.right)
        {
            name += "Right";
            lookDir = "Right";
        }  
        else if(dir == Vector2.down)
        {
            name += "Front";
            lookDir = "Front";
        }
        else if(dir == Vector2.up)
        {
            name += "Back";
            lookDir = "Back";
        }  
        thisAnimator.Play(name);
    }

    public void OnAttackAnimationEnd()
    {
        string idleName = "Idle";
        if(lookDir == "Left")
            PlayAnimation(Vector2.left, idleName, 0.5f);
        else if(lookDir == "Right")
            PlayAnimation(Vector2.right, idleName, 0.5f);
        else if(lookDir == "Front")
            PlayAnimation(Vector2.down, idleName, 0.5f);
        else if(lookDir == "Back")
            PlayAnimation(Vector2.up, idleName, 0.5f);
    }
}
