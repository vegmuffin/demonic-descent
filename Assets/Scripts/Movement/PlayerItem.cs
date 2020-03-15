﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    private Transform childItem;
    private TrailRenderer childTrail;
    private SpriteRenderer childSprite;

    [SerializeField] private float windUpOffset;
    [SerializeField] private AnimationCurve windUpSpeedCurve;
    [SerializeField] private AnimationCurve strikeSpeedCurve;
    [SerializeField] private AnimationCurve fadeOutSpeedCurve;

    [HideInInspector] public GameObject target;

    private void Awake()
    {
        childItem = transform.GetChild(0);
        childTrail = childItem.GetComponent<TrailRenderer>();
        childSprite = childItem.GetComponent<SpriteRenderer>();
    }

    private void Collision()
    {
        Vector2 itemPos = childItem.position;
        Vector2 targetPos = target.transform.position;

        if(CheckDistance(itemPos, targetPos))
        {
            CancelInvoke("Collision");
            transform.GetComponent<UnitMovement>().OnAttackAnimation();
            StartCoroutine(FadeOut());
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

        Debug.Log(zAngle);
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

                childTrail.enabled = true;

                InvokeRepeating("Collision", 0.1f, 0.1f);

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
            Debug.Log(timer);
            childItem.localPosition = Vector2.Lerp(startPos, endPos, timer);

            timer += Time.deltaTime * strikeSpeedCurve.Evaluate(timer);
            if(timer >= 1f)
            {
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

        while(timer <= 1f)
        {
            childSprite.color = Color.Lerp(startColor, endColor, timer);

            timer += Time.deltaTime * fadeOutSpeedCurve.Evaluate(timer);
            if(timer >= 1f)
            {
                childSprite.color = endColor;
                //childTrail.Clear();

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
        return (pointA - pointB).sqrMagnitude < 1*1 ? true : false;
    }

}
