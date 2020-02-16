using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Transform tr;
    private float timer = 0f;

    private void Awake()
    {
        tr = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveAlongPath(List<Vector3Int> path)
    {
        for(int i = 0; i < path.Count-1; ++i)
        {
            Vector2 currentPos = new Vector2(path[i].x, path[i].y);
            Vector2 futurePos = new Vector2(path[i+1].x, path[i+1].y);

            StartCoroutine(MoveLerp(currentPos, futurePos));
        }
    }

    private IEnumerator MoveLerp(Vector2 startPos, Vector2 endPos)
    {
        while(timer <= 1f)
        {
            tr.position = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime;
            if(timer >= 1f)
            {
                timer = 0;
                tr.position = endPos;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }
}
