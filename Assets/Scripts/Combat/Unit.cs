using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    public Sprite combatQueueImage;
    public bool isEnemy;
    [HideInInspector] public int maxHealth;
    public int health;
    public int combatPoints;
    public int damage;
    public string acronym;
    public string realName;
    [SerializeField] private int aggroRange = default;
    [SerializeField] private float deathTime = default;

    private Color aggroColor;
    [HideInInspector] public Tilemap movementTilemap;
    [HideInInspector] public int currentCombatPoints;
    [HideInInspector] public int hoveringCombatPoints;

    [HideInInspector] public bool isDying = false;
    [HideInInspector] public bool isAttacking = false;
    private Animator thisAnimator;
    private string lookDir = "Front";

    private SpriteRenderer spriteRenderer;
    private Shader defaultShader;
    private Shader flashShader;

    private void Awake()
    {
        currentCombatPoints = combatPoints;
        if(transform.tag == "Player")
        {
            thisAnimator = transform.GetComponent<Animator>();
            UIManager.instance.InitiatePlayerUI(health, combatPoints, damage);
        }
        spriteRenderer = transform.GetComponent<SpriteRenderer>();

        defaultShader = Shader.Find("Sprites/Default");
        flashShader = Shader.Find("GUI/Text Shader");
        maxHealth = health;
    }

    private void Start()
    {
        if(isEnemy)
        {
            movementTilemap = transform.parent.GetChild(1).GetChild(0).GetComponent<Tilemap>();
            aggroColor = CombatManager.instance.aggroColor;

            AggroGrid();
        }
    }

    // Painting the aggro range grid. If a player steps on the grid, combat will be initiated.
    private void AggroGrid()
    {
        Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        MovementManager.instance.GenerateGrid(pos, aggroRange, movementTilemap, aggroColor);
    }

    // Update mouse cursor.
    private void OnMouseOver()
    {
        if(isEnemy)
        {
            if(CursorManager.instance.currentState != CursorManager.CursorStates.ATTACK)
            {
                if(isDying)
                {
                    CursorManager.instance.SetCursor("DEFAULT");
                    CursorManager.instance.currentState = CursorManager.CursorStates.DEFAULT;
                }  
                else
                {
                    CursorManager.instance.currentState = CursorManager.CursorStates.ATTACK;
                    CursorManager.instance.SetCursor("ATTACK");
                }
            }
        }
    }

    private void OnMouseExit()
    {
        CursorManager.instance.currentState = CursorManager.CursorStates.DEFAULT;
        CursorManager.instance.SetCursor("DEFAULT");
        CursorManager.instance.SetAttackCursorDir("left");
    }

    public void OnDamage(Unit damageDealer, int damageAmount)
    {
        this.health -= damageAmount;
        StartCoroutine(FlashSprite());

        if(health <= 0)
        {
            // Creating a log entry.
            string logEntry = "<color=#3399FF>" + damageDealer.realName + "</color> killed <color=#FF9900>" + this.realName + "</color> with a final blow of <color=#FF3333>" + damageAmount + "</color> damage!";
            UILog.instance.NewLogEntry(logEntry);

            // Calling UI to dispose of the dead unit.
            int index = CombatManager.instance.GetObjectIndex(gameObject);
            UIManager.instance.AlternativeDeathChange(gameObject);

            // If the cursor is on the dying unit, update it. NEEDS TO BE UPDATED SINCE NOW THE CURSOR IS AN OVERLAY IMAGE.
            var box = transform.GetComponent<BoxCollider2D>();
            if(CursorManager.instance.currentState == CursorManager.CursorStates.ATTACK && box.bounds.Contains((Vector2)CursorManager.instance.transform.position))
                OnMouseExit();

            isDying = true;
            MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), true);
            CombatManager.instance.RemoveFromQueue(transform.gameObject);
            Destroy(transform.parent.gameObject, deathTime);


            if(gameObject.name == "Player")
            {
                logEntry = "<color=#FFFFFF>GAME OVER!</color>";
                UILog.instance.NewLogEntry(logEntry);
            }
            else
            {
                // Execute looting!
                var looting = transform.GetComponent<LootTable>();
                looting.DropLoot();
            }
        }
        else
        {
            // If it's not combat and we are attacking (out of range attacks, sneak attacks or something), initiate it.
            if(!GameStateManager.instance.CheckState("COMBAT"))
            {
                GameStateManager.instance.ChangeState("COMBAT");

                foreach(GameObject anotherEnemy in CombatManager.instance.enemyList)
                    anotherEnemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

                currentCombatPoints = combatPoints;
                CombatManager.instance.InitiateCombat();
            }
            else
            {
                // We are not dying and it's combat, update UI and wait after attack.
                bool isPlayer = false;
                if(transform.tag == "Player")
                    isPlayer = true;

                if(GameStateManager.instance.CheckState("COMBAT"))
                {
                    int index = CombatManager.instance.GetObjectIndex(gameObject);
                    UIManager.instance.HealthChange(gameObject, health, isPlayer, false);
                }
                StartCoroutine(CombatManager.instance.WaitAfterAttack(this));
            }

            // Creating a log entry.
            string logEntry = "<color=#3399FF>" + damageDealer.realName + "</color> dealt <color=#FF3333>" + damageAmount + "</color> damage to <color=#FF9900>" + this.realName + "</color>.";
            UILog.instance.NewLogEntry(logEntry);
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

    private IEnumerator FlashSprite()
    {
        float timer = 0f;
        float flashRate = CombatManager.instance.flashSpriteRate;

        // Flashing the sprite white using a different shader.
        spriteRenderer.material.shader = flashShader;
        Color revertColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * flashRate;

            if(timer >= 1f)
            {
                spriteRenderer.material.shader = defaultShader;
                spriteRenderer.color = revertColor;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }
}
