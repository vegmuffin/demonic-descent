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
        List<GridNode> pathToPlayer = GetShortestAvailablePath(startCoord, endCoord);
        pathToPlayer.Reverse();

        //Debug.Log(pathToPlayer.Count);

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
            
            skeletonUnitMovement.StartMoving(path, path.Length, false);
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
            if(path.Length + 2 <= skeletonUnit.currentCombatPoints)
            {
                isAttacking = true;
                Vector2 overlapCirclePos = new Vector2(endCoord.x+0.5f, endCoord.y+0.5f);

                Collider2D[] cols = Physics2D.OverlapCircleAll(overlapCirclePos, 0.33f);
                foreach(Collider2D col in cols)
                {
                    if(col.gameObject.tag == "Player")
                    {
                        skeletonUnit.currentTarget = col.gameObject;
                        break;
                    }
                }
            }
            skeletonUnitMovement.StartMoving(path, path.Length, isAttacking);
        }
    }

    private List<GridNode> GetShortestAvailablePath(Vector3Int startPos, Vector3Int endPos)
    {
        List<List<GridNode>> paths = new List<List<GridNode>>();
        Vector3Int playerPos = endPos;

        // Bottom
        endPos = new Vector3Int(playerPos.x, playerPos.y - 1, 0);
        paths.Add(MovementManager.instance.Pathfinding(startPos, endPos, 0, true, skeletonUnit.movementTilemap));

        // Left
        endPos = new Vector3Int(playerPos.x - 1, playerPos.y, 0);
        paths.Add(MovementManager.instance.Pathfinding(startPos, endPos, 0, true, skeletonUnit.movementTilemap));

        // Top
        endPos = new Vector3Int(playerPos.x, playerPos.y + 1, 0);
        paths.Add(MovementManager.instance.Pathfinding(startPos, endPos, 0, true, skeletonUnit.movementTilemap));

        // Right
        endPos = new Vector3Int(playerPos.x + 1, playerPos.y, 0);
        paths.Add(MovementManager.instance.Pathfinding(startPos, endPos, 0, true, skeletonUnit.movementTilemap));

        int[] pathLengths = new int[4];
        for(int i = 0; i < paths.Count; ++i)
        {
            pathLengths[i] = paths[i].Count;
        }

        int minIndex = 0;
        for(int i = 0; i < paths.Count; ++i)
        {
            if((pathLengths[i] < pathLengths[minIndex] || pathLengths[minIndex] == 0) && pathLengths[i] != 0)
                minIndex = i;
        }

        return paths[minIndex];
    }
}
