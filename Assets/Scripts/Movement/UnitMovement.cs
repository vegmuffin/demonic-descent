using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitMovement : MonoBehaviour
{
    private Transform tr;
    private float timer = 0f;

    [HideInInspector] public int remainingMoves = default;
    [HideInInspector] public bool isMoving = false;

    private Unit unit;

    private void Awake()
    {
        tr = transform;
        unit = transform.GetComponent<Unit>();
    }

    public IEnumerator MoveAlongPath(Vector3Int[] path, int combatPoints, bool isAttacking, GameObject target)
    {
        while(remainingMoves > 0)
        {
            if(GameStateManager.instance.gameState == GameStateManager.GameStates.COMBAT && CombatManager.instance.initiatingCombatState)
            {
                MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), false);
                MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)path[0].x, (int)path[0].y, 0), true);
                yield break;
            }
            Vector2 futurePos = new Vector2(path[remainingMoves-1].x, path[remainingMoves-1].y);
            --remainingMoves;
            yield return StartCoroutine(MoveLerp(tr.position, futurePos));
        }
        
        isMoving = false;

        // Updating past and current tiles.
        MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), false);
        MovementManager.instance.UpdateTileWalkability(new Vector3Int((int)path[0].x, (int)path[0].y, 0), true);

        if(GameStateManager.instance.gameState == GameStateManager.GameStates.COMBAT && !CombatManager.instance.initiatingCombatState)
        {
            unit.currentCombatPoints -= combatPoints;

            if(isAttacking && target != null)
            {
                // Do attacking stuff
            }

            if(unit.currentCombatPoints > 0)
            {
                CombatManager.instance.ExecuteTurns();
            }
            else
            {
                CombatManager.instance.NextTurn();
            }
        }
        else if(GameStateManager.instance.gameState != GameStateManager.GameStates.COMBAT)
        {
            var temp = GameStateManager.instance.gameState;
            GameStateManager.instance.gameState = GameStateManager.instance.previousGameState;
            GameStateManager.instance.previousGameState = temp;
        }
        
    }

    private IEnumerator MoveLerp(Vector2 startPos, Vector2 endPos)
    {
        while(timer <= 1f)
        {
            tr.position = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * MovementManager.instance.unitSpeed;
            if(timer >= 1f)
            {
                timer = 0;
                tr.position = endPos;

                // Checking for combat engagement.
                if(CheckEngagement())
                {
                    GameStateManager.instance.previousGameState = GameStateManager.instance.gameState;
                    GameStateManager.instance.gameState = GameStateManager.GameStates.COMBAT;

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
    }

    private bool CheckEngagement()
    {
        if(!unit.isEnemy && GameStateManager.instance.gameState != GameStateManager.GameStates.COMBAT)
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

}
