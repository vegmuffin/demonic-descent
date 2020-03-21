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

    public void CameraShake(float angle, float magnitude)
    {
        Vector3 startPos = transform.position;

        Debug.Log("Angle: " + angle);
        angle *= Mathf.Deg2Rad;
        float x = startPos.x;
        float y = startPos.y;
        x += magnitude * Mathf.Cos(angle);
        y += magnitude * Mathf.Sin(angle);

        Vector3 endPos = new Vector3(x, y, startPos.z);
        
        StartCoroutine(Shake(startPos, endPos));
    }

    private IEnumerator Shake(Vector3 startPos, Vector3 endPos)
    {
        float timer = 0f;
        AnimationCurve curveRef = CombatManager.instance.cameraShakeSpeedCurve;
        while(timer <= 1f)
        {
            if(timer > 0.5f)
                transform.position = Vector3.Lerp(endPos, startPos, timer);
            else
                transform.position = Vector3.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * CombatManager.instance.cameraShakeSpeedCurve.Evaluate(timer);

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
