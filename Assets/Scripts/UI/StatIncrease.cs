using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatIncrease : MonoBehaviour
{
    private TMP_Text thisTMP;
    private AnimationCurve statFadeCurve;
    private Color startColor;
    private Color endColor;
    private float timer = 0f;
    private bool isCoroutineRunning = false;

    private void Awake()
    {
        thisTMP = transform.GetComponent<TMP_Text>();
        statFadeCurve = UIManager.instance.statFadeCurve;
        startColor = UIManager.instance.statAdditionColor;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    public void Enable(int addition)
    {
        thisTMP.color = startColor;
        thisTMP.text = "+" + addition.ToString();
        if(isCoroutineRunning)
        {
            timer = 0f;
        }
        else
        {
            isCoroutineRunning = true;
            StartCoroutine(StatIncreaseCoroutine());
        }
    }

    private IEnumerator StatIncreaseCoroutine()
    {
        while(timer <= 1f)
        {
            timer += Time.deltaTime * statFadeCurve.Evaluate(timer);
            thisTMP.color = Color.Lerp(startColor, endColor, timer);

            if(timer >= 1f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
            
        }
        yield break;
    }
}
