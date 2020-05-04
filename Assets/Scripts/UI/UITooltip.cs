using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float hoverTimer = 0f;
    private bool tooltipGenerated = false;
    private bool coroutineStarted = false;

    // Getting all the variables locally in the Awake() method to avoid constantly referencing the singletons.
    private float tooltipThreshold;
    private Color endTooltipColor;
    private Color startTooltipColor;
    private float tooltipFadeInRate;
    private Vector2 previousMousePosition = Vector2.zero;
    private RectTransform thisRect;
    private GameObject tooltip;
    private RectTransform canvasRect;
    
    [SerializeField] [TextArea] private string tooltipText = default;
    [SerializeField] private int fontSize = default;

    private RectTransform tooltipRect;
    private TMP_Text tooltipTMP;
    private RectTransform tooltipTMPRect;
    private Image tooltipImage;

    private void Awake()
    {
        tooltipThreshold = UIAnimations.instance.tooltipThreshold;
        endTooltipColor = UIAnimations.instance.endTooltipColor;
        tooltipFadeInRate = UIAnimations.instance.tooltipFadeInRate;
        startTooltipColor = UIAnimations.instance.startTooltipColor;
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();

        thisRect = transform.GetComponent<RectTransform>();
        tooltip = UIManager.instance.tooltip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(TooltipTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(coroutineStarted)
        {
            StopAllCoroutines();
            coroutineStarted = false;
            hoverTimer = 0f;
        }

        if(tooltipGenerated)
        {
            tooltipGenerated = false;
            Destroy(tooltipRect.gameObject, 0.2f);
            tooltipRect = null;
            tooltipTMP = null;
            tooltipImage = null;
            tooltipTMPRect = null;
        }
        
    }

    private IEnumerator TooltipTimer()
    {
        coroutineStarted = true;
        
        while(hoverTimer <= tooltipThreshold)
        {
            // Reset timer if the mouse is moving.
            if(!IsMouseStatic())
            {
                hoverTimer = 0f;
            }
            
            hoverTimer += Time.deltaTime;

            if(hoverTimer >= tooltipThreshold)
            {
                hoverTimer = 0f;
                coroutineStarted = false;
                tooltipGenerated = true;

                CreateTooltip(Vector2.zero);

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    private void CreateTooltip(Vector2 position)
    {
        GameObject tooltipInstance = Instantiate(tooltip, Vector2.zero, Quaternion.identity, transform);
        tooltipInstance.name = gameObject.name + "Tooltip";
        tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        tooltipTMPRect = tooltipInstance.transform.Find("TooltipText").GetComponent<RectTransform>();
        tooltipTMP = tooltipTMPRect.GetComponent<TMP_Text>();
        tooltipTMP.text = tooltipText;
        tooltipImage = tooltipInstance.GetComponent<Image>();

        StartCoroutine(FadeInTooltip());
        PositionTooltip();

    }

    // Might be a good practice to allow very slight movement.
    private bool IsMouseStatic()
    {
        Vector2 currentMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        
        if(currentMousePos == previousMousePosition)
        {
            return true;
        }
        else
        {
            previousMousePosition = currentMousePos;
            return false;
        }
            
    }

    private IEnumerator FadeInTooltip()
    {
        float timer = 0f;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * tooltipFadeInRate;
            tooltipImage.color = Color.Lerp(startTooltipColor, endTooltipColor, timer);

            if(timer >= 1f)
            {
                tooltipImage.color = endTooltipColor;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    private bool PositionTooltip()
    {
        // Reisizing according to the text.
        tooltipTMP.fontSize = fontSize - 4;

        float sizeX = tooltipTMP.preferredWidth;
        float sizeY = tooltipTMP.preferredHeight;

        if(tooltipTMP.preferredWidth > tooltipRect.rect.width)
        {
            sizeX = 200f;
        }

        tooltipTMPRect.sizeDelta = new Vector2(sizeX, sizeY);
        tooltipRect.sizeDelta = tooltipTMPRect.sizeDelta;
        
        float xMin = thisRect.rect.xMin;
        float xMax = thisRect.rect.xMax;
        float yMin = thisRect.rect.yMin;
        float yMax = thisRect.rect.yMax;

        tooltipRect.anchoredPosition = new Vector2(xMin - tooltipRect.rect.width, yMax);
        if(!IsTooltipRectValid())
        {
            tooltipRect.anchoredPosition = new Vector2(xMax, yMax);
            if(!IsTooltipRectValid())
            {
                tooltipRect.anchoredPosition = new Vector2(xMin - tooltipRect.rect.width, yMin);
                if(!IsTooltipRectValid())
                {
                    tooltipRect.anchoredPosition = new Vector2(xMax, yMin - thisRect.rect.height);
                    if(!IsTooltipRectValid())
                        return false;
                }
            }
        }

        return true;
    }

    private bool IsTooltipRectValid()
    {
        Vector3[] worldCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(worldCorners);

        if(worldCorners[0].x >= canvasRect.rect.width || worldCorners[0].x <= 0 || worldCorners[0].y >= canvasRect.rect.height || worldCorners[0].y <= 0)
            return false;
        if(worldCorners[1].x >= canvasRect.rect.width || worldCorners[1].x <= 0 || worldCorners[1].y >= canvasRect.rect.height || worldCorners[1].y <= 0)
            return false;
        if(worldCorners[2].x >= canvasRect.rect.width || worldCorners[2].x <= 0 || worldCorners[2].y >= canvasRect.rect.height || worldCorners[2].y <= 0)
            return false;
        if(worldCorners[3].x >= canvasRect.rect.width || worldCorners[3].x <= 0 || worldCorners[3].y >= canvasRect.rect.height || worldCorners[3].y <= 0)
            return false;

        // All corners valid.
        return true;
    }
}
