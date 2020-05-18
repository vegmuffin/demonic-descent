using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomUnit : MonoBehaviour
{
    public List<GameObject> unitPool = new List<GameObject>();

    public GameObject GenerateRandomUnit()
    {
        // Select random unit from the pool and instantiate it.
        int ran = Random.Range(0, unitPool.Count);
        GameObject randomGO = Instantiate(unitPool[ran], Vector3.zero, Quaternion.identity);

        Vector3 qmPos = transform.position;
        Transform goTransform = randomGO.transform;
        for(int i = 0; i < goTransform.childCount; ++i)
        {
            Transform child = goTransform.GetChild(i);
            if(child.tag == "Enemy")
            {
                child.position = qmPos;
                break;
            }
        }

        return randomGO;
    }

    public void Dispose()
    {
        Destroy(gameObject);
    }

}
