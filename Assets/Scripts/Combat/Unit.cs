using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public Texture2D combatQueueImage;
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
    private Animator thisAnimator;

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
        if(animationName == "Idle")
        {
            if(dir == Vector2.left)
            {
                thisAnimator.Play("IdleAnimationLeft");
            } 
            else if(dir == Vector2.right)
            {
                thisAnimator.Play("IdleAnimationRight");
            }
            else if(dir == Vector2.up)
            {
                thisAnimator.Play("IdleAnimationBack");
            }
            else if(dir == Vector2.down)
            {
                thisAnimator.Play("IdleAnimationFront");
            }
        }
        else if(animationName == "Move")
        {
            if(dir == Vector2.left)
            {
                thisAnimator.Play("MovingAnimationLeft");
            }
            else if(dir == Vector2.right)
            {
                thisAnimator.Play("MovingAnimationRight");
            }
            else if(dir == Vector2.up)
            {
                thisAnimator.Play("MovingAnimationBack");
            }
            else if(dir == Vector2.down)
            {
                thisAnimator.Play("MovingAnimationFront");
            }
        }

    }
}
