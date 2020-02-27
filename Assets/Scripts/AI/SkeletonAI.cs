using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    private Unit skeletonUnit;
    private UnitMovement skeletonUnitMovement;

    private void Awake()
    {
        skeletonUnit = transform.GetComponent<Unit>();
        skeletonUnitMovement = transform.GetComponent<UnitMovement>();
    }

    public void BeginAI()
    {

    }

    private void Walk()
    {
        
    }
}
