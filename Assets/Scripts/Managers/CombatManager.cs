using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public Color aggroColor;
    [HideInInspector] public List<GameObject> enemyList = new List<GameObject>();
    [HideInInspector] public List<GameObject> combatQueue = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GatherEnemies();
    }

    private void GatherEnemies()
    {
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemyList.Add(enemy);
        }
    }

    public void QueueUnits()
    {
        GameObject player = GameObject.Find("Player");
        combatQueue.Add(player);
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            combatQueue.Add(enemy);

        for(int i = 0; i < combatQueue.Count; ++i)
        {
            for(int j = i+1; j < combatQueue.Count; ++j)
            {
                if(combatQueue[i].GetComponent<Unit>().combatPoints < combatQueue[j].GetComponent<Unit>().combatPoints)
                {
                    var temp = combatQueue[i];
                    combatQueue[i] = combatQueue[j];
                    combatQueue[j] = temp;;
                }
            }
        }

        foreach(GameObject unit in combatQueue)
        {
            Debug.Log(unit);
        }
    }

}