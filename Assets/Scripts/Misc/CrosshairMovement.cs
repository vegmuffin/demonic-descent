using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairMovement : MonoBehaviour
{
    [SerializeField] private float speed = default;
    [SerializeField] private float speedDecay = default;
    private Transform tr;
    private float timer = 0f;
    private bool isMoving = false;

    private void Awake()
    {
        tr = transform;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.D))
        {
            Vector2 dir = Vector2.zero;
            if(Input.GetKeyDown(KeyCode.A))
            {
                dir = new Vector2(-1, 0); // Left
            } 
            else if(Input.GetKeyDown(KeyCode.S))
            {
                dir = new Vector2(0, -1); // Back
            } 
            else if(Input.GetKeyDown(KeyCode.W))
            {
                dir = new Vector2(0, 1); // Forward
            } 
            else if(Input.GetKeyDown(KeyCode.D))
            {
                dir = new Vector2(1, 0); // Right
            } 

            Vector2 startPos = tr.position;
            Vector2 endPos = startPos + dir;
            if(!isMoving)
            {
                StartCoroutine(MoveLerp(startPos, endPos));
            }
        }
    }

    private IEnumerator MoveLerp(Vector2 startPos, Vector2 endPos)
    {
        isMoving = true;
        float dynamicSpeed = speed;
        while(timer <= 1f)
        {
            tr.position = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * dynamicSpeed;
            dynamicSpeed -= dynamicSpeed * speedDecay;
            if(timer >= 1f)
            {
                timer = 0;
                tr.position = endPos;
                isMoving = false;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }
}
