using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private Camera mainCamera;

    private void Awake()
    {
        instance = this;
        mainCamera = transform.GetComponent<Camera>();
    }

    public void CameraShake(float angle, float magnitude, float speed)
    {
        Vector3 startPos = transform.position;

        float x = startPos.x;
        float y = startPos.y;
        x += magnitude * Mathf.Cos(angle);
        y += magnitude * Mathf.Sin(angle);

        Vector3 endPos = new Vector3(x, y, startPos.z);
        
        StartCoroutine(Shake(startPos, endPos, speed));
    }

    private IEnumerator Shake(Vector3 startPos, Vector3 endPos, float speed)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            if(timer > 0.5f)
                transform.position = Vector3.Lerp(endPos, startPos, timer);
            else
                transform.position = Vector3.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * speed;

            if(timer >= 1f)
            {
                transform.position = startPos;
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
