using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAnimations : MonoBehaviour
{
    public static UIAnimations instance;

    [Header("Panel show/hide animation")]
    [SerializeField] private AnimationCurve panelShowSpeedCurve;
    [SerializeField] private AnimationCurve panelHideSpeedCurve;
    [SerializeField] private float queuePanelXOffset;
    [SerializeField] private float queuePanelYOffset;
    [Header("Damage UI text animation")]
    [SerializeField] private float textBounceHeight;
    [SerializeField] private Color textFlashColor;
    [SerializeField] private float textFlashSpeed;
    [Header("Element hide animation")]
    [SerializeField] private AnimationCurve elementHideSpeedCurve;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator ShowQueue(RectTransform elementRect)
    {
        float timer = 0f;

        float x = elementRect.anchoredPosition.x;
        float y = elementRect.anchoredPosition.y;
        Vector2 startPos = new Vector2(x, y - queuePanelYOffset);
        Vector2 endPos = new Vector2(-queuePanelXOffset, y - queuePanelYOffset);

        while(timer <= 1f)
        {
            timer += Time.deltaTime * panelShowSpeedCurve.Evaluate(timer);
            elementRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                elementRect.anchoredPosition = endPos;

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }

    public IEnumerator HideQueue(RectTransform elementRect)
    {
        float timer = 0f;

        float x = elementRect.anchoredPosition.x;
        float y = elementRect.anchoredPosition.y;
        Vector2 startPos = new Vector2(x, y);
        Vector2 endPos = new Vector2(x + queuePanelXOffset*4, y);

        while(timer <= 1f)
        {
            timer += Time.deltaTime * panelHideSpeedCurve.Evaluate(timer);
            elementRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                elementRect.anchoredPosition = endPos;

                for(int i = 0; i < elementRect.transform.childCount; ++i)
                    Destroy(elementRect.transform.GetChild(i).gameObject, 5f); // Give it time to not fuck up.
                
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }

    public IEnumerator HideElement(RectTransform elementRect)
    {
        float timer = 0f;

        float x = elementRect.anchoredPosition.x;
        float y = elementRect.anchoredPosition.y;
        Vector2 startPos = new Vector2(x, y);
        Vector2 endPos = new Vector2(x + queuePanelXOffset*4, y);

        while(timer <= 1f)
        {
            timer += Time.deltaTime * elementHideSpeedCurve.Evaluate(timer);
            elementRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                elementRect.anchoredPosition = endPos;
                Destroy(elementRect.gameObject, 5f);
                
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }
    

    public IEnumerator HealthFlash(TMP_Text healthText)
    {
        float timer = 0f;

        Color defaultColor = healthText.color;
        healthText.color = textFlashColor;

        Vector4 defaultMargin = healthText.margin;
        Vector4 bounceMargin = new Vector4(defaultMargin.x, defaultMargin.y, defaultMargin.z, textBounceHeight);
        
        while(timer <= 1)
        {
            if(timer >= 0.5f)
            {
                healthText.margin = Vector4.Lerp(bounceMargin, defaultMargin, timer);
            } 
            else
            {
                healthText.margin = Vector4.Lerp(defaultMargin, bounceMargin, timer);
            }
            
            timer += Time.deltaTime * textFlashSpeed;
            
            if(timer >= 1)
            {
                healthText.color = defaultColor;
                healthText.margin = defaultMargin;
                timer = 0f;
                yield break;
            }
            else
                yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
        
        
        yield break;
    }
}
