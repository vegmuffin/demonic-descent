using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Currently unused
public class UILog : MonoBehaviour
{
    public static UILog instance;

    [SerializeField] private GameObject logText = default;

    private Vector2 firstTextPos;
    private RectTransform thisRect;
    private float yThreshold;

    private void Awake()
    {
        instance = this;
        RectTransform textRect = logText.GetComponent<RectTransform>();
        firstTextPos = new Vector2(textRect.anchoredPosition.x, textRect.anchoredPosition.y);
        thisRect = transform.GetComponent<RectTransform>();
        yThreshold = thisRect.rect.max.y;
    }

    public void NewLogEntry(string text)
    {
        GameObject newTextInstance = Instantiate(logText, Vector2.zero, Quaternion.identity, transform);
        RectTransform newTextRect = newTextInstance.GetComponent<RectTransform>();
        newTextRect.anchoredPosition = firstTextPos;
        newTextInstance.GetComponent<TMP_Text>().text = text;

        float textHeight = newTextRect.rect.height;
        RepositionText(textHeight);
    }

    private void RepositionText(float textHeight)
    {
        int childCount = transform.childCount;

        for(int i = 0; i < childCount-1; ++i)
        {
            RectTransform childRect = transform.GetChild(i).GetComponent<RectTransform>();
            Vector2 newChildPos = new Vector2(childRect.anchoredPosition.x, childRect.anchoredPosition.y + textHeight);
            childRect.anchoredPosition = newChildPos;

            if(childRect.anchoredPosition.y >= yThreshold)
            {
                Destroy(childRect.gameObject);
            }
        }
    }

    public void ClearLog()
    {
        int childCount = transform.childCount;

        for(int i = 0; i < childCount; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
