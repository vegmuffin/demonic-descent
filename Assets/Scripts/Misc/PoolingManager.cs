using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager instance;

    private Dictionary<GameObject, List<GameObject>> poolingObjects;

    private void Awake()
    {
        instance = this;
        poolingObjects = new Dictionary<GameObject, List<GameObject>>();
    }

    public GameObject GetObject(GameObject objectType)
    {
        if (poolingObjects.ContainsKey(objectType))
        {
            
        }
    }
}
