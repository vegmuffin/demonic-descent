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

    public void CameraShake(float angle, float magnitude, int frameCount)
    {
        Vector3 startPos = transform.position;

        float x = startPos.x;
        float y = startPos.y;
        x += magnitude * Mathf.Cos(angle);
        y += magnitude * Mathf.Sin(angle);

        Vector3 endPos = new Vector3(x, y, startPos.z);
        
        StartCoroutine(ShakeForFrames(startPos, endPos, frameCount));
    }

    private IEnumerator ShakeForFrames(Vector3 startPos, Vector3 endPos, int frames)
    {
        transform.position = endPos;
        for(int i = 0; i < frames; ++i)
            yield return new WaitForSecondsRealtime(Time.deltaTime);

        transform.position = startPos;
        yield break;
    }
}
