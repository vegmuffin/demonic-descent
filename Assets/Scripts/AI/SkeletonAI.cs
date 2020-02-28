using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    private Unit skeletonUnit;
    private UnitMovement skeletonUnitMovement;
    private GameObject player;

    private void Awake()
    {
        skeletonUnit = transform.GetComponent<Unit>();
        skeletonUnitMovement = transform.GetComponent<UnitMovement>();
        player = GameObject.Find("Player");
    }

    public void BeginAI()
    {
        Walk();
    }

    private void Walk()
    {
        Vector3Int startCoord = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        Vector3Int endCoord = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 0);
        List<GridNode> pathToPlayer = MovementManager.instance.Pathfinding(startCoord, endCoord, 0, true, skeletonUnit.movementTilemap, false, true);
        pathToPlayer.Reverse();

        // This means we didn't reach to attack.
        if(pathToPlayer.Count > skeletonUnit.currentCombatPoints)
        {
            List<GridNode> newPath = new List<GridNode>();
            for(int i = 0; i < skeletonUnit.currentCombatPoints; ++i)
            {
                newPath.Add(pathToPlayer[i]);
            }
            Vector3Int[] path = new Vector3Int[newPath.Count];
            for(int i = 0; i < newPath.Count; ++i)
            {
                path[i] = new Vector3Int((int)newPath[i].position.x, (int)newPath[i].position.y, 0);
            }
            skeletonUnitMovement.remainingMoves = 0;
            
            StartCoroutine(skeletonUnitMovement.MoveAlongPath(startCoord, path, path.Length, false, null));
        }
        else
        {
            // This means we have reached to attack. We also need to check for combat points if we have to attack.
            Vector3Int[] path = new Vector3Int[pathToPlayer.Count];
            for(int i = 0; i < pathToPlayer.Count; ++i)
            {
                path[i] = new Vector3Int((int)pathToPlayer[i].position.x, (int)pathToPlayer[i].position.y, 0);
            }
            bool isAttacking = false;
            GameObject target = null;
            if(path.Length + 2 <= skeletonUnit.currentCombatPoints)
            {
                isAttacking = true;
                Vector2 overlapCirclePos = new Vector2(endCoord.x+0.5f, endCoord.y+0.5f);
                target = Physics2D.OverlapCircle(overlapCirclePos, 0.33f).gameObject;
            }
            skeletonUnitMovement.remainingMoves = 0;
            StartCoroutine(skeletonUnitMovement.MoveAlongPath(startCoord, path, path.Length, isAttacking, target));
        }
    }
}
