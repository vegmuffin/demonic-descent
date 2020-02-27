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

    }

    private void Walk()
    {
        Vector3Int startCoord = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        Vector3Int endCoord = new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y, 0);
        List<GridNode> pathToPlayer = MovementManager.instance.Pathfinding(startCoord, endCoord, 0, false, skeletonUnit.movementTilemap, false, true);
        if(pathToPlayer.Count > skeletonUnit.currentCombatPoints)
        {
            List<GridNode> newPath = new List<GridNode>();
            for(int i = 0; i < skeletonUnit.currentCombatPoints; ++i)
            {
                newPath[i] = pathToPlayer[i];
            }
            // Move along this path, else, move along the original path
        }
    }
}
