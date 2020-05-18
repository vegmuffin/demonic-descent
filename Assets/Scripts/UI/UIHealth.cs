using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [HideInInspector] public bool isEmpty = false;
    [SerializeField] private Sprite fullHealthImage = default;
    [SerializeField] private Sprite emptyHealthImage = default;

    [Header("Damage flash")]
    [SerializeField] private AnimationCurve flashSpeedAnimationCurve = default;
    [SerializeField] private Color flashColor = default;

    [Header("Heal flash")]
    [SerializeField] private AnimationCurve healSpeedAnimationCurve = default;
    [SerializeField] private Color healColor = default;

    [Header("Increase flash")]
    [SerializeField] private AnimationCurve increaseSpeedAnimationCurve = default;
    [SerializeField] private Color increaseColor = default;

    private Image img;
    private Color startingColor;

    private void Awake()
    {
        img = transform.GetComponent<Image>();
        startingColor = img.color;
    }

    public void ChangeImageToEmpty()
    {
        img.sprite = emptyHealthImage;
    }

    public IEnumerator Deplete()
    {
        float timer = 0f;
        img.color = flashColor;
        isEmpty = true;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * flashSpeedAnimationCurve.Evaluate(timer);

            if(timer >= 1f)
            {
                img.color = Color.black;
                img.sprite = emptyHealthImage;

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    public IEnumerator Heal()
    {
        float timer = 0f;
        img.sprite = fullHealthImage;
        img.color = healColor;
        isEmpty = false;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * healSpeedAnimationCurve.Evaluate(timer);

            if(timer >= 1f)
            {
                img.color = startingColor;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
        yield break;
    }

    public IEnumerator Increase()
    {
        float timer = 0f;
        Color endColor = img.color;
        img.color = increaseColor;

        while(timer <= 1f)
        {
            timer += Time.deltaTime * increaseSpeedAnimationCurve.Evaluate(timer);
            img.color = Color.Lerp(increaseColor, endColor, timer);

            if(timer >= 1f)
            {
                img.color = endColor;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }
}
