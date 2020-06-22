using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueElement : MonoBehaviour
{
    [HideInInspector] public GameObject attachedGameObject;
    [HideInInspector] public int whichRound;
    [HideInInspector] public float amountToMove;
}
