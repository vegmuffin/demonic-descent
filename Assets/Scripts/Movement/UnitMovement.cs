using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    private Transform tr;
    private float timer = 0f;

    public int speed;
    [HideInInspector] public int remainingMoves = default;

    private void Awake()
    {
        tr = transform;
    }

    public IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        while(remainingMoves > 0)
        {
            Vector2 futurePos = new Vector2(path[remainingMoves-1].x, path[remainingMoves-1].y);
            --remainingMoves;
            yield return StartCoroutine(MoveLerp(tr.position, futurePos));
        }

        remainingMoves = speed;
        var temp = GameStateManager.instance.gameState;
        GameStateManager.instance.gameState = GameStateManager.instance.previousGameState;
        GameStateManager.instance.previousGameState = temp;
    }

    private IEnumerator MoveLerp(Vector2 startPos, Vector2 endPos)
    {
        while(timer <= 1f)
        {
            tr.position = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * MovementManager.instance.unitSpeed;
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
