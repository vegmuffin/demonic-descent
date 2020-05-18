using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot", menuName = "ScriptableObjects/LootScriptableObject", order = 1)]
public class Loot : ScriptableObject
{
    public int chanceToDrop;
    public GameObject drop;
    public int customValue;
}