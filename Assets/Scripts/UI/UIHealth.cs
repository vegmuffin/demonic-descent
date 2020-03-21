using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [HideInInspector] public bool isEmpty = false;
    [SerializeField] private Sprite fullHealthImage = default;
    [SerializeField] private Sprite emptyHealthImage = default;
    [SerializeField] private AnimationCurve flashSpeedAnimationCurve = default;
    [SerializeField] private Color flashColor;

    private Image img;

    private void Awake()
    {
        img = transform.GetComponent<Image>();
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
}
