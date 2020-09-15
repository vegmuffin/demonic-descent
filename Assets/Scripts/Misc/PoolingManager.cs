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

    // Gets an object from a pool instead of resource-heavy instantiate. Object is returned by disabling it.
    public GameObject GetObject(GameObject objectType, Vector2 pos)
    {
        if (poolingObjects.ContainsKey(objectType))
        {
            // search for existing object
            for (int i = 0; i < poolingObjects[objectType].Count; ++i)
            {
                GameObject go = poolingObjects[objectType][i];
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    go.transform.position = pos;
                    return go;
                }
            }
            // if we have reached here, there is no available object
            List<GameObject> objects = poolingObjects[objectType];
            GameObject newObject = Instantiate(objectType, pos, Quaternion.identity);
            objects.Add(newObject);
            return newObject;
        }
        else
        {
            // objectType is new type
            List<GameObject> newTypeObjects = new List<GameObject>();
            GameObject newObject = Instantiate(objectType, pos, Quaternion.identity);
            newTypeObjects.Add(newObject);
            poolingObjects.Add(objectType, newTypeObjects);
            return newObject;
        }
    }
}
