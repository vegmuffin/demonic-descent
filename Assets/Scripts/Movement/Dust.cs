using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dust : MonoBehaviour
{
    private Vector3 startScale = new Vector3(0.4f, 0.4f, 0.4f);
    private Vector3 endScale = new Vector3(0.01f, 0.01f, 0.01f);
    private Color endColor = new Color(255, 255, 255, 0);
    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = transform.GetComponent<SpriteRenderer>();
    }
   
    public IEnumerator DustMove(Vector2 dir, float depleteSpeed)
    {
        float timer = 0f;
        Vector2 startPos = (Vector2)transform.position;
        Vector2 endPos = startPos + dir/2;

        while(timer < 1f)
        {
            transform.position = Vector2.Lerp(startPos, endPos, timer);
            transform.localScale = Vector3.Lerp(startScale, endScale, timer);
            sprite.color = Color.Lerp(Color.white, endColor, timer);

            timer += Time.deltaTime * depleteSpeed;

            if(timer >= 1f)
            {
                gameObject.SetActive(false);
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }
}
