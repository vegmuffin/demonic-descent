using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitMovement : MonoBehaviour
{
    [HideInInspector] public bool isMoving = false;

    private Unit unit;
    private Vector2 lastDirection = Vector2.zero;
    private AnimationCurve unitSpeedCurve;
    private Transform unitSprite;
    private GameObject dustParticle;
    private float dustDepleteSpeed;

    private float moveTimer = 0f;
    private Vector3Int[] path;
    private int pathLength = 0;
    private bool isAttacking = false;
    private int currentTileIndex = 0;
    private Vector2 movingPos = Vector2.zero;
    private bool engagement = false;

    private Vector3 startRot = Vector3.zero;
    private Vector3 endRot = Vector3.zero;
    private bool rotateBool = false;
    private readonly float rotateRange = 4f;

    private PlayerVisuals visuals;

    private void Awake()
    {
        unit = transform.GetComponent<Unit>();
        unitSpeedCurve = MovementManager.instance.unitSpeedCurve;
        unitSprite = transform.Find("UnitSprite");
        dustParticle = MovementManager.instance.dustParticle;
        dustDepleteSpeed = MovementManager.instance.dustDepleteSpeed;
        visuals = transform.GetComponent<PlayerVisuals>();
    }

    private void Update()
    {
        Move();
    }

    public void StartMoving(Vector3Int[] path, int pathLength, bool isAttacking)
    {
        this.path = path;
        this.pathLength = pathLength;
        this.isAttacking = isAttacking;

        // There is no path to traverse, move along to actions that occur at the end of moving.
        if(pathLength == 0)
        {
            MoveEnd();
        }
        else
        {
            // Setting up variables which are needed to begin the movement.
            isMoving = true;
            Vector3Int unitPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            moveTimer = 0f;
            currentTileIndex = 0;
            movingPos = new Vector2(path[currentTileIndex].x, path[currentTileIndex].y);

            // This little logic (used later as well) swings the player's sprite a bit to appear somewhat funky!
            if(rotateBool)
                endRot = new Vector3(0, 0, rotateRange);
            else
                endRot = new Vector3(0, 0, -rotateRange);

            // Updating the direction and the walkability of our past tile.
            UpdateDirection(unitPos, path[currentTileIndex]);
            MovementManager.instance.UpdateTileWalkability(unitPos, true, null);

            // Spawns dust in the opposite direction to where we are going. Needs pooling!
            Vector2 startDustPos = new Vector2(unitPos.x + 0.5f, unitPos.y + 0.2f);
            Transform dust = PoolingManager.instance.GetObject(dustParticle, startDustPos).transform;
            StartCoroutine(dust.GetComponent<Dust>().DustMove(lastDirection*-1, dustDepleteSpeed));

            // Update eye visuals.
            if(transform.CompareTag("Player"))
                visuals.MoveEyes(lastDirection);
        }
    }

    // Runs every frame when moving. Used to be a coroutine but that proved to be very messy. And perhaps even laggy.
    private void Move()
    {
        if(isMoving)
        {
            // Lerp both the position and the rotation to make moving and swinging appear smooth.
            moveTimer += Time.deltaTime * unitSpeedCurve.Evaluate(moveTimer);
            transform.position = Vector2.Lerp(transform.position, movingPos, moveTimer);
            unitSprite.eulerAngles = Vector3.Lerp(startRot, endRot, moveTimer);

            // Timer needs to reset since we are approaching the next tile.
            if(moveTimer >= 1f)
            {
                // Dust again.
                Vector2 startDustPos = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.2f);
                Transform dust = PoolingManager.instance.GetObject(dustParticle, startDustPos).transform;
                StartCoroutine(dust.GetComponent<Dust>().DustMove(lastDirection*-1, dustDepleteSpeed));

                moveTimer = 0f;

                // Updating the current player position.
                if(transform.CompareTag("Player"))
                {
                    Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
                    PlayerController.instance.playerPos = pos;
                }

                // Removing combat points as one tile to move costs one combat point.
                if(GameStateManager.instance.CheckState("COMBAT"))
                {
                    unit.currentCombatPoints -= 1;
                    UIManager.instance.UpdateCombatPoints(unit.currentCombatPoints-1, unit.currentCombatPoints, unit.combatPoints, gameObject);
                }

                // If we enganged in combat or we approached
                engagement = unit.CheckEngagement();
                if(currentTileIndex == pathLength-1 || engagement)
                {
                    MoveEnd();
                    return;
                }

                ++currentTileIndex;

                // Earlier explained swinging logic.
                rotateBool = !rotateBool;
                if(currentTileIndex == pathLength)
                    endRot = Vector3.zero;
                else if(rotateBool)
                    endRot = new Vector3(0, 0, rotateRange);
                else
                    endRot = new Vector3(0, 0, -rotateRange);
                
                // Updating the unit position to be exactly the specified position to avoid floating points. Updating the future position as well for the next iteration.
                transform.position = movingPos;
                movingPos = new Vector2(path[currentTileIndex].x, path[currentTileIndex].y);

                if (transform.CompareTag("Player"))
                {
                    Vector2 dir = lastDirection;
                    UpdateDirection(PlayerController.instance.playerPos, new Vector3Int((int)movingPos.x, (int)movingPos.y, 0));

                    if (lastDirection != dir)
                        visuals.MoveEyes(lastDirection);
                }
            }
        }
    }

    private void MoveEnd()
    {
        // We arrived at the end of our path or we engaged in combat. Clean up some things.
        unitSprite.eulerAngles = Vector3.zero;
        transform.position = movingPos;
        isMoving = false;
        Vector3Int currentPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        MovementManager.instance.UpdateTileWalkability(currentPos, false, gameObject);

        // Initiate combat if we stepped on attack tiles.
        if(engagement)
        {
            CombatManager.instance.InitiateCombat();
            isAttacking = false;
        }

        if((GameStateManager.instance.CheckState("COMBAT") && !CombatManager.instance.initiatingCombatState) || isAttacking)
        {
            // If we are attacking (regardless of combat or not), play attack animations.
            if(isAttacking)
            {
                GameObject target = unit.currentTarget;
                UpdateDirection(new Vector3Int((int)transform.position.x, (int)transform.position.y, 0), new Vector3Int((int)target.transform.position.x, (int)target.transform.position.y, 0));
                unit.OnAttack(lastDirection);    
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

        engagement = false;

        lastDirection = Vector2.zero;
        if (transform.CompareTag("Player"))
            visuals.MoveEyes(Vector2.zero);
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
