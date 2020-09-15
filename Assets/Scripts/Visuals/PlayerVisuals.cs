using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    private Transform eyeMiddle;
    private Transform leftEye;
    private Transform rightEye;

    private Vector2 beginLoc;
    private Vector2 endLoc = Vector2.zero;
    private Vector2 startLoc = Vector2.zero;
    private bool eyesMoving = false;
    private float eyesTimer = 0f;

    private float blinkTimer = 0f;
    private float nextBlink = 0f;
    private bool blinking = false;
    private float blinkingTimer = 0f;
    private Vector3 squashScale;
    private Vector3 scale;
    private int blinkPhase = 1;

    [SerializeField] private float eyeOffset = default;
    [SerializeField] private AnimationCurve eyeMovementRate = default;
    [SerializeField] private float blinkFreq = default;
    [SerializeField] private float blinkVariation = default;
    [SerializeField] private float blinkSpeed = default;

    private void Awake()
    {
        eyeMiddle = transform.Find("UnitSprite").Find("EyesMiddle");
        leftEye = eyeMiddle.Find("EyeLeft");
        rightEye = eyeMiddle.Find("EyeRight");

        beginLoc = eyeMiddle.localPosition;
        nextBlink = blinkFreq + Random.Range(-blinkVariation, blinkVariation);
        scale = rightEye.localScale; // doesn't matter which one, they're the same
        squashScale = new Vector3(scale.x, 0.02f, scale.z);
    }

    private void Update()
    {
        EyeMovement();
        BlinkTimer();
        Blinking();
    }

    public void MoveEyes(Vector2 dir)
    {
        startLoc = eyeMiddle.localPosition;
        Vector2 addition = dir == Vector2.left ? new Vector2(-eyeOffset, 0f) :
                           dir == Vector2.right ? new Vector2(eyeOffset, 0f) :
                           dir == Vector2.up ? new Vector2(0f, eyeOffset) :
                           dir == Vector2.down ? new Vector2(0f, -eyeOffset) :
                           Vector2.zero; // <-- this is for when player stops moving
        endLoc = beginLoc + addition;

        if(eyesMoving)
            eyesTimer = 0f;
        else
            eyesMoving = true;
    }

    private void EyeMovement()
    {
        if(eyesMoving)
        {
            eyesTimer += Time.deltaTime * eyeMovementRate.Evaluate(eyesTimer);
            eyeMiddle.localPosition = Vector2.Lerp(startLoc, endLoc, eyesTimer);

            if(eyesTimer >= 1f)
            {
                eyeMiddle.localPosition = endLoc;
                eyesMoving = false;
                eyesTimer = 0f;
            }
        }
    }

    private void BlinkTimer()
    {
        if(!eyesMoving && !blinking)
        {
            blinkTimer += Time.deltaTime;
            if(blinkTimer >= nextBlink)
            {
                nextBlink = blinkFreq + Random.Range(-blinkVariation, blinkVariation);
                blinking = true;
                blinkTimer = 0f;
            }
        }
    }

    private void Blinking()
    {
        if(blinking)
        {
            blinkingTimer += Time.deltaTime * blinkSpeed;

            Vector3 sc = blinkPhase == 1 ? Vector3.Lerp(scale, squashScale, blinkingTimer) :
                                           Vector3.Lerp(squashScale, scale, blinkingTimer);
            leftEye.localScale = sc;
            rightEye.localScale = sc;

            if(blinkingTimer >= 1f && blinkPhase == 2)
            {
                blinking = false;
                blinkingTimer = 0f;
                leftEye.localScale = scale;
                rightEye.localScale = scale;
            }
            else if (blinkPhase == 1)
            {
                blinkPhase = 2;
                blinkingTimer = 0f;
            }
        }
    }
}
