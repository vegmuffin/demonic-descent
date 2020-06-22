using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitMovement : MonoBehaviour
{
    private Transform tr;

    [HideInInspector] public int remainingMoves = default;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public GameObject target = null;

    private Unit unit;
    private Vector2 lastDirection = Vector2.zero;

    private void Awake()
    {
        tr = transform;
        unit = transform.GetComponent<Unit>();
    }

    // Path is divided into grid nodes, the outer enumerator walks through all of the nodes.
    public IEnumerator MoveAlongPath(Vector3Int startNode, Vector3Int[] path, int combatPoints, bool isAttacking, GameObject target)
    {
        this.target = target;

        // While we are still moving, lerp-move towards the end of the path.
        while(remainingMoves < combatPoints)
        {
            // If combat has been initiated, we have to come to a stop.
            if(GameStateManager.instance.CheckState("COMBAT") && CombatManager.instance.initiatingCombatState)
            {
                isMoving = false;
                MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), false);
                MovementManager.instance.UpdateTileWalkability(startNode, true);
                /* if(tr.tag == "Player")
                    unit.PlayAnimation(lastDirection, "Idle", 0.5f); */
                yield break;
            }
            
            if(GameStateManager.instance.CheckState("COMBAT"))
            {
                unit.currentCombatPoints -= 1;
                UIManager.instance.UpdateCombatPoints(unit.currentCombatPoints, unit.combatPoints, gameObject);
            }

            // Get next grid node.
            Vector2 futurePos = new Vector2(path[remainingMoves].x, path[remainingMoves].y);

            // Updating variables.
            UpdateDirection(new Vector3Int((int)tr.position.x, (int)transform.position.y, 0), path[remainingMoves]);
            /* if(!unit.isEnemy)
                unit.PlayAnimation(lastDirection, "Moving", 2); */

            ++remainingMoves;

            // Starting the inner enumerator.
            yield return StartCoroutine(MoveLerp(tr.position, futurePos));
        }

        /* if(!isAttacking && tr.tag == "Player")
            unit.PlayAnimation(lastDirection, "Idle", 0.5f); */
        isMoving = false;

        // Updating past and current tiles.
        MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), false);
        if(path.Length != 0)
            MovementManager.instance.UpdateTileWalkability(startNode, true);

        // If it's combat or if we are attacking, proceed further.
        if((GameStateManager.instance.CheckState("COMBAT") && !CombatManager.instance.initiatingCombatState) || isAttacking)
        {
            // If we are attacking (regardless of combat or not), play attack animations.
            if(isAttacking && target != null)
            {
                UpdateDirection(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), new Vector3Int((int)target.transform.position.x, (int)target.transform.position.y, 0));

                // ------------------ HAS TO BE CHANGED WHEN THERE ARE ENEMY ANIMATIONS IN PLACE
                var item = transform.GetComponent<AttackItem>();
                item.target = target;
                unit.isAttacking = true;
                item.BasicAttack(lastDirection, target.transform.position);
                    
            }
            // If we are not attacking, that means we have reached the target grid node. Check if we still have combat points.
            else if(unit.currentCombatPoints > 0)
            {
                // If it's an enemy, all the combat points have been expended if we have reached the target tile and we are not attacking. Subject to change.
                if(unit.gameObject.name != "Player")
                    CombatManager.instance.NextTurn();
                else
                    CombatManager.instance.ExecuteTurns();
            }
            // If we do not have any combat points left, pass the turn along.
            else
            {
                CombatManager.instance.NextTurn();
            }
        }
        // If it's not Combat and we are not attacking, update game states back from MOVING to EXPLORING.
        else if(!GameStateManager.instance.CheckState("COMBAT"))
        {
            GameStateManager.instance.ChangeState("EXPLORING");
        }
        
        yield break;
    }

    // This is triggered by the animation event.
    public void OnAttackAnimation(float angle)
    {
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            unit.currentCombatPoints -= 2; // Basic attack costs 2 combat points.
            UIManager.instance.UpdateCombatPoints(unit.currentCombatPoints, unit.combatPoints, gameObject);
        }

        // Updating health and calling the OnDamage method.
        var targetUnit = target.GetComponent<Unit>();

        // DAMAGE AMOUNT
        int damageAmount = unit.damage;
        targetUnit.OnDamage(unit, damageAmount);

        CameraManager.instance.CameraShake(angle, 0.8f);
    }

    // The inner enumerator is lerping from one grid node to the other to make smooth movement.
    private IEnumerator MoveLerp(Vector2 startPos, Vector2 endPos)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            tr.position = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * MovementManager.instance.unitSpeed;
            if(timer >= 1f)
            {
                tr.position = endPos;

                // Checking for combat engagement.
                if(CheckEngagement())
                {
                    // Combat has been engaged, setting states, clearing tiles.
                    GameStateManager.instance.ChangeState("COMBAT");

                    foreach(GameObject anotherEnemy in CombatManager.instance.enemyList)
                        anotherEnemy.GetComponent<Unit>().movementTilemap.ClearAllTiles();

                    isMoving = false;
                    unit.currentCombatPoints = unit.combatPoints;
                    CombatManager.instance.InitiateCombat();
                }

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    private bool CheckEngagement()
    {
        // If the GameObject of this unit has Player tag and if it isn't combat, check for exisiting aggro tiles to initiate combat.
        if(tr.tag == "Player" && !GameStateManager.instance.CheckState("COMBAT"))
        {
            Vector3Int playerPos = new Vector3Int((int)Mathf.Floor(tr.position.x), (int)Mathf.Floor(tr.position.y), 0);
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

    private void UpdateDirection(Vector3Int current, Vector3Int target)
    {
        if(current.x > target.x)
            lastDirection = Vector2.left;
        else if(current.x < target.x)
            lastDirection = Vector2.right;
        else if(current.y > target.y)
            lastDirection = Vector2.down;
        else if(current.y < target.y)
            lastDirection = Vector2.up;
    }

}
