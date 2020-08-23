using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackItem : MonoBehaviour
{
    private Transform childItem;
    private TrailRenderer childTrail;
    private SpriteRenderer childSprite;

    [SerializeField] private float windUpOffset = default;
    [SerializeField] private AnimationCurve windUpSpeedCurve = default;
    [SerializeField] private AnimationCurve strikeSpeedCurve = default;
    [SerializeField] private AnimationCurve fadeOutSpeedCurve = default;

    [HideInInspector] public GameObject target;

    private Vector3 primarylocalPosition;
    private Quaternion primaryRotation;
    private bool shouldCheckCollision = false;
    private Unit thisUnit;

    private void Awake()
    {
        childItem = transform.Find("Item");
        primarylocalPosition = childItem.localPosition;
        primaryRotation = childItem.rotation;
        childTrail = childItem.GetComponent<TrailRenderer>();
        childTrail.emitting = false;
        childSprite = childItem.GetComponent<SpriteRenderer>();
        thisUnit = transform.GetComponent<Unit>();
    }

    private void FixedUpdate()
    {
        Collision();
    }

    private void Collision()
    {
        if(shouldCheckCollision)
        {
            Vector2 itemPos = childItem.position;
            Vector2 targetPos = target.transform.position;

            if(CheckDistance(itemPos, targetPos))
            {
                shouldCheckCollision = false;
                thisUnit.OnAttackEnd(childItem.localRotation.eulerAngles.z);
                StartCoroutine(FadeOut());
            }
        }
        
    }

    public void BasicAttack(Vector2 dir, Vector2 targetPos)
    {
        Vector2 startPos = childItem.localPosition;
        Vector2 futurePos = Vector2.zero;
        targetPos = new Vector2(targetPos.x + 0.5f, targetPos.y + 0.5f); // Pivot

        if(dir == Vector2.up || dir == Vector2.down)
            futurePos = new Vector2(startPos.x - windUpOffset, startPos.y);
        else
            futurePos = new Vector2(startPos.x, startPos.y - windUpOffset);

        Vector2 rotPos = new Vector2(futurePos.x + transform.position.x, futurePos.y + transform.position.y);

        float x1 = rotPos.x;
        float x2 = targetPos.x;
        float y1 = rotPos.y;
        float y2 = targetPos.y;
        float zAngle = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;

        Quaternion startRot = childItem.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, zAngle);

        StartCoroutine(WindUp(startPos, futurePos, startRot, endRot, zAngle));
    }

    private IEnumerator WindUp(Vector2 startPos, Vector2 endPos, Quaternion startRot, Quaternion endRot, float angle)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            childItem.localPosition = Vector2.Lerp(startPos, endPos, timer);
            childItem.localRotation = Quaternion.Lerp(startRot, endRot, timer);

            timer += Time.deltaTime * windUpSpeedCurve.Evaluate(timer);
            if(timer >= 1f)
            {
                childItem.localPosition = endPos;
                childItem.localRotation = endRot;

                angle *= Mathf.Deg2Rad;
                float x = endPos.x + 5.5f * Mathf.Cos(angle);
                float y = endPos.y + 5.5f * Mathf.Sin(angle);
                Vector2 targetPos = new Vector2(x, y);

                childTrail.emitting = true;

                shouldCheckCollision = true;

                yield return StartCoroutine(Strike(endPos, targetPos));
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        yield break;
    }

    private IEnumerator Strike(Vector2 startPos, Vector2 endPos)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            childItem.localPosition = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * strikeSpeedCurve.Evaluate(timer);
            if(timer >= 1f)
            {
                childItem.localPosition = primarylocalPosition;
                childItem.rotation = primaryRotation;

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        yield break;
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color startColor = childSprite.color;
        Color endColor = new Color(255, 255, 255, 0);

        GradientAlphaKey alphaKey = childTrail.colorGradient.alphaKeys[0];
        float startAlpha = alphaKey.alpha;
        float endAlpha = 10f;

        while(timer <= 1f)
        {
            childSprite.color = Color.Lerp(startColor, endColor, timer);
            alphaKey.alpha = Mathf.Lerp(startAlpha, endAlpha, timer);

            timer += Time.deltaTime * fadeOutSpeedCurve.Evaluate(timer);
            if(timer >= 1f)
            {
                childTrail.emitting = false;
                childSprite.color = startColor;
                thisUnit.isAttacking = false;

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        yield break;
    }

    private bool CheckDistance(Vector2 pointA, Vector2 pointB)
    {
        return (pointA - pointB).sqrMagnitude < 0.8*0.8 ? true : false;
    }

}
