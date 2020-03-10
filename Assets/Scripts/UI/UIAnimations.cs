using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAnimations : MonoBehaviour
{
    public static UIAnimations instance;

    [SerializeField] private float queuePanelShowSpeed;
    [SerializeField] private float queuePanelHideSpeed;
    [SerializeField] private float singleElementHideSpeed;
    [SerializeField] private float queuePanelXOffset;
    [SerializeField] private float queuePanelYOffset;
    [Space]
    [SerializeField] private float textBounceHeight;
    [SerializeField] private Color textFlashColor;
    [SerializeField] private float textFlashTime;
    private float flashTimer = 0f;

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
            timer += Time.deltaTime * queuePanelShowSpeed;
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
            timer += Time.deltaTime * queuePanelHideSpeed;
            elementRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                elementRect.anchoredPosition = endPos;

                for(int i = 0; i < elementRect.transform.childCount; ++i)
                    Destroy(elementRect.transform.GetChild(i).gameObject);
                
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
            timer += Time.deltaTime * singleElementHideSpeed;
            elementRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);

            if(timer >= 1f)
            {
                elementRect.anchoredPosition = endPos;

                for(int i = 0; i < elementRect.transform.childCount; ++i)
                    Destroy(elementRect.transform.GetChild(i).gameObject);
                
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
        Color defaultColor = healthText.color;
        healthText.color = textFlashColor;

        Vector4 defaultMargin = healthText.margin;
        healthText.margin = new Vector4(defaultMargin.x, defaultMargin.y, defaultMargin.z, textBounceHeight);
        
        while(flashTimer <= textFlashTime)
        {
            flashTimer += Time.deltaTime;

            if(flashTimer >= textFlashTime)
            {
                healthText.color = defaultColor;
                healthText.margin = defaultMargin;
                flashTimer = 0f;
                yield break;
            }
            else
                yield return new WaitForSecondsRealtime(Time.deltaTime);
        }
        
        
        yield break;
    }
}
