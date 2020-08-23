using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(UnitMovement))]
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

    private SpriteRenderer spriteRenderer;
    private Shader defaultShader;
    private Shader flashShader;

    [HideInInspector] public GameObject currentTarget;

    private void Awake()
    {
        currentCombatPoints = combatPoints;
        if(transform.tag == "Player")
        {
            thisAnimator = transform.GetComponent<Animator>();
        }
        spriteRenderer = transform.Find("UnitSprite").GetComponent<SpriteRenderer>();

        defaultShader = Shader.Find("Sprites/Default");
        flashShader = Shader.Find("GUI/Text Shader");
        maxHealth = health;
        health = maxHealth;
        currentCombatPoints = combatPoints;
        currentTarget = gameObject;
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
        MovementManager.instance.GenerateAttackGrid(pos, aggroRange, movementTilemap, aggroColor);
    }

    public void OnAttack(Vector2 dir)
    {
        var item = transform.GetComponent<AttackItem>();
        item.target = currentTarget;
        isAttacking = true;
        item.BasicAttack(dir, currentTarget.transform.position);
    }

    public void OnAttackEnd(float angle)
    {
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            currentCombatPoints -= 2; // Basic attack costs 2 combat points.
            UIManager.instance.UpdateCombatPoints(currentCombatPoints-2, currentCombatPoints, combatPoints, gameObject);
        }

        // Updating health and calling the OnDamage method.
        var targetUnit = currentTarget.GetComponent<Unit>();

        // DAMAGE AMOUNT
        targetUnit.OnDamage(this, damage);

        CameraManager.instance.CameraShake(angle, 0.8f);
    }

    public void OnDamage(Unit damageDealer, int damageAmount)
    {
        int previousHealth = this.health;
        this.health -= damageAmount;
        StartCoroutine(FlashSprite());

        if(health <= 0)
        {
            /* // Creating a log entry.
            string logEntry = "<color=#3399FF>" + damageDealer.realName + "</color> killed <color=#FF9900>" + this.realName + "</color> with a final blow of <color=#FF3333>" + damageAmount + "</color> damage!";
            UILog.instance.NewLogEntry(logEntry); */

            // Calling UI to dispose of the dead unit.
            int index = CombatManager.instance.GetObjectIndex(gameObject);
            UIManager.instance.DeathChange(gameObject);

            isDying = true;
            MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), true, null);
            CombatManager.instance.RemoveFromQueue(transform.gameObject);
            Destroy(transform.parent.gameObject, deathTime);


            if(gameObject.name == "Player")
            {
                /* logEntry = "<color=#FFFFFF>GAME OVER!</color>";
                UILog.instance.NewLogEntry(logEntry); */
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
                    UIManager.instance.HealthChange(gameObject, previousHealth, health, isPlayer, false);
                }
                StartCoroutine(CombatManager.instance.WaitAfterAttack(damageDealer));
            }

            /* // Creating a log entry.
            string logEntry = "<color=#3399FF>" + damageDealer.realName + "</color> dealt <color=#FF3333>" + damageAmount + "</color> damage to <color=#FF9900>" + this.realName + "</color>.";
            UILog.instance.NewLogEntry(logEntry); */
        }
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

    public bool CheckEngagement()
    {
        // If the GameObject of this unit has Player tag and if it isn't combat, check for exisiting aggro tiles to initiate combat.
        if(transform.CompareTag("Player") && !GameStateManager.instance.CheckState("COMBAT"))
        {
            Vector3Int playerPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), 0);
            foreach(GameObject enemy in CombatManager.instance.enemyList)
            {
                Tilemap unitAggroTilemap = enemy.GetComponent<Unit>().movementTilemap;
                if(unitAggroTilemap.HasTile(playerPos))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
