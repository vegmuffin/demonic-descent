using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldFlashScript : MonoBehaviour
{
    private float startDelay = 1f;
    private float popRate = 0.2f;
    float timer = 0f;
    private bool elapsed = false;

    private Color blackTransparent = new Color(0f, 0f, 0f, 0f);
    private Color whiteTransparent = new Color(1f, 1f, 1f, 0f);

    private List<TMP_Text> allTexts = new List<TMP_Text>();
    private List<Color> allStartColors = new List<Color>();
    private List<Color> allEndColors = new List<Color>();

    private void Awake()
    {
        var text = transform.GetComponent<TMP_Text>();
        allTexts.Add(text);
        allStartColors.Add(Color.black);
        allEndColors.Add(blackTransparent);
        for(int i = 0; i < transform.childCount; ++i)
        {
            var textComponent = transform.GetChild(i).GetComponent<TMP_Text>();
            allTexts.Add(textComponent);
            if(textComponent.name.Contains("White"))
            {
                allStartColors.Add(Color.white);
                allEndColors.Add(whiteTransparent);
            }
            else
            {
                allStartColors.Add(Color.black);
                allEndColors.Add(blackTransparent);
            }
        }
    }

    private void Update()
    {
        if(!elapsed)
        {
            timer += Time.deltaTime;
            for(int i = 0; i < allTexts.Count; ++i)
                allTexts[i].color = Color.Lerp(allEndColors[i], allStartColors[i], timer/popRate);
            if(timer >= startDelay)
            {
                elapsed = true;
                StartCoroutine(Flash());
            }
        }
    }

    private IEnumerator Flash()
    {
        float rate = UIAnimations.instance.goldTextFadeRate;
        var rect = transform.GetComponent<RectTransform>();

        Vector2 startPos = rect.anchoredPosition;
        Vector2 offset = UIAnimations.instance.goldTextOffset;
        Vector2 endPos = new Vector2(startPos.x + offset.x, startPos.y + offset.y);

        float timer = 0f;
        while(timer <= 1f)
        {
            timer += Time.deltaTime * rate;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer);
            for(int i = 0; i < allTexts.Count; ++i)
                allTexts[i].color = Color.Lerp(allStartColors[i], allEndColors[i], timer);

            if(timer >= 1f)
            {
                Destroy(gameObject);
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }
}
