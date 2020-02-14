using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairShaderScript : MonoBehaviour
{
    [SerializeField] private Material mat;
    [Space]
    [SerializeField] private float noiseSpeed;
    [SerializeField] private float noiseCeiling;
    [SerializeField] private float noiseFloor;
    [Space]
    [SerializeField] private float scaleSpeed;
    [SerializeField] private float scaleCeiling;
    [SerializeField] private float scaleFloor;
    private float noiseAmount;
    private float noiseTimer = 0f;
    private float scaleAmount;
    private int noiseDirectionMultiplier = 1;
    private int scaleDirectionalMultiplier = 1;

    private Transform tr;

    private void Awake()
    {
        tr = transform;
        noiseAmount = mat.GetFloat("_NoiseAmount");
        scaleAmount = tr.localScale.x; // or Y, doesn't matter
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateMaterial();
        UpdateScale();
    }

    private void UpdateMaterial()
    {
        noiseTimer += Time.deltaTime * noiseDirectionMultiplier * noiseSpeed;
        if((noiseTimer >= noiseCeiling && noiseDirectionMultiplier == 1) || (noiseTimer <= noiseFloor && noiseDirectionMultiplier == -1))
        {
            noiseDirectionMultiplier *= -1;
        }
    
        noiseAmount = noiseTimer;
        mat.SetFloat("_NoiseAmount", noiseAmount);
    }

    private void UpdateScale()
    {
        scaleAmount += Time.deltaTime * scaleDirectionalMultiplier * scaleSpeed;
        if((scaleAmount >= scaleCeiling && scaleDirectionalMultiplier == 1) || (scaleAmount <= scaleFloor && scaleDirectionalMultiplier == -1))
        {
            scaleDirectionalMultiplier *= -1;
        }

        Vector2 newScale = new Vector2(scaleAmount, scaleAmount);
        tr.localScale = newScale;
    }
}
