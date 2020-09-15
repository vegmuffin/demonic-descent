using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEyeBlip : MonoBehaviour
{
    private Vector2 startLocLeft;
    private Vector2 startLocRight;
    private Transform blipLeft;
    private Transform blipRight;

    [SerializeField] private Vector2 maxOffset = default;


    private void Awake()
    {
        blipLeft = transform.Find("EyeLeft").GetChild(0);
        blipRight = transform.Find("EyeRight").GetChild(0);
        startLocRight = blipRight.localPosition;
        startLocLeft = blipLeft.localPosition;
    }

    private void Update()
    {
        Blip();
    }

    private void Blip()
    {
        float randX = Random.Range(-maxOffset.x, maxOffset.x);
        float randY = Random.Range(-maxOffset.y, maxOffset.y);

        Vector2 offsetLocLeft = new Vector2(startLocLeft.x + randX, startLocLeft.y + randY);
        Vector2 offsetLocRight = new Vector2(startLocRight.x + randX, startLocRight.y + randY);
        blipLeft.localPosition = offsetLocLeft;
        blipRight.localPosition = offsetLocRight;
    }
}
