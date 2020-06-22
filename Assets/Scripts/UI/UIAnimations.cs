using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct RearrangementElement{

    public Vector2 startPos;
    public Vector2 endPos;
    public RectTransform rect;

    public RearrangementElement(Vector2 startPos, Vector2 endPos, RectTransform rect)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.rect = rect;
    }
}

public class UIAnimations : MonoBehaviour
{
    public static UIAnimations instance;

    [Header("Panel show/hide animation")]
    [SerializeField] private AnimationCurve panelShowSpeedCurve = default;
    [SerializeField] private AnimationCurve panelHideSpeedCurve = default;
    [SerializeField] private float queuePanelXOffset = default;
    [SerializeField] private float queuePanelYOffset = default;

    [Header("Damage UI text animation")]
    [SerializeField] private float textBounceHeight = default;
    [SerializeField] private Color textFlashColor = default;
    [SerializeField] private float textFlashSpeed = default;

    [Header("Element DYING animation")]
    [Header("Curves for all stages of the combat queue")]
    [SerializeField] private AnimationCurve elementDyingSpeedCurve = default;

    [Header("Element REARRANGEMENT animation")]
    [SerializeField] private AnimationCurve elementRearrangementSpeedCurve = default;

    [Header("NEW element animation")]
    [SerializeField] private AnimationCurve elementNewSpeedCurve = default;

    [Header("Element HIDE animation")]
    [SerializeField] private AnimationCurve elementHideSpeedCruve = default;

    [Header("Element SHIFT animation")]
    [SerializeField] private AnimationCurve elementShiftSpeedCurve = default;

    [Header("Element LAST animation")]
    [SerializeField] private AnimationCurve elementLastSpeedCurve = default;

    [Header("Gold text flash animation")]
    [SerializeField] private Color goldFlashColor = default;
    [SerializeField] private float goldFlashSpeed = default;
    [SerializeField] private float goldBounceHeight = default;

    [Header("Stat increase animation")]
    [SerializeField] private float statIncreaseScale = default;
    [SerializeField] private Color statFlashColor = default;
    [SerializeField] private AnimationCurve statFlashSpeedCurve = default;

    [Header("Tooltip animation")]
    public float tooltipThreshold;
    public Color endTooltipColor;
    public Color startTooltipColor;
    public float tooltipFadeInRate;

    [HideInInspector] public List<RearrangementElement> secondElementList = new List<RearrangementElement>();

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

        yield break;
    }

    // I'll have to get right on this since right now the combat ends when one unit is left in combat -- it is not a smooth experience.
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

        yield break;
    }

    // This coroutine is started whenever we want to move elements around in the combat queue.
    // IMPORTANT - this does not support async deaths so this may be a problem in the future. But not now :p

    // Stages go like this: DYING -> REARRANGING -> NEW -> HIDE -> SHIFT -> LAST
    public IEnumerator RearrangeElements(string whichStage, List<RearrangementElement> elementList)
    {
        AnimationCurve curve = null;
        if(whichStage == "DYING")
            curve = elementDyingSpeedCurve;
        else if(whichStage == "REARRANGING")
            curve = elementRearrangementSpeedCurve;
        else if(whichStage == "NEW")
            curve = elementNewSpeedCurve;
        else if(whichStage == "HIDE")
            curve = elementHideSpeedCruve;
        else if(whichStage == "SHIFT")
            curve = elementShiftSpeedCurve;
        else if(whichStage == "LAST")
            curve = elementLastSpeedCurve;
        else
            yield break;
        
        float timer = 0f;

        if(elementList.Count != 0)
        {
            while(timer <= 1f)
            {
                for(int i = 0; i < elementList.Count; ++i)
                    elementList[i].rect.anchoredPosition = Vector2.Lerp(elementList[i].startPos, elementList[i].endPos, timer);
                
                timer += Time.deltaTime * curve.Evaluate(timer);
                if(timer >= 1f)
                {
                    for(int i = 0; i < elementList.Count; ++i)
                        elementList[i].rect.anchoredPosition = elementList[i].endPos;

                    // Calling this coroutine again with other elements.
                    if(whichStage == "DYING")
                    {
                        foreach(RearrangementElement ele in elementList)
                            Destroy(ele.rect.gameObject);

                        StartCoroutine(RearrangeElements("REARRANGING", secondElementList));
                    }
                    else if(whichStage == "REARRANGING")
                    {
                        CreateNewElements("REARRANGING");
                    }
                    else if(whichStage == "HIDE")
                    {
                        foreach(RearrangementElement ele in elementList)
                            Destroy(ele.rect.gameObject);
                        ShiftElements();
                    }
                    else if(whichStage == "SHIFT")
                    {
                        CreateNewElements("SHIFT");
                    }
                }
                else
                {
                    yield return new WaitForSecondsRealtime(Time.deltaTime);
                }
            }
        }

        // If we are still in combat, execute new turns.
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            if(whichStage == "NEW")
            {
                if(CombatManager.instance.currentUnit.currentCombatPoints <= 0)
                    CombatManager.instance.NextTurn();
                else
                    CombatManager.instance.ExecuteTurns();
            }
            else if(whichStage == "LAST")
                CombatManager.instance.ExecuteTurns();
        }


        yield break;
    }

    private void CreateNewElements(string whichStage)
    {
        List<List<GameObject>> rounds = UIManager.instance.rounds;

        int lastRoundElements = rounds[rounds.Count-1].Count - 1;
        UIManager.instance.RefillQueue(lastRoundElements, whichStage);

        secondElementList.Clear();
    }
    
    private void ShiftElements()
    {
        List<RearrangementElement> newElementList = new List<RearrangementElement>();
        List<List<GameObject>> rounds = UIManager.instance.rounds;
        int currentRound = CombatManager.instance.currentRound-1;
        bool isNewRound = UIManager.instance.newRound;
        for(int i = currentRound; i < rounds.Count; ++i)
        {
            for(int j = 0; j < rounds[i].Count; ++j)
            {
                if(i == currentRound && j == 0)
                {
                    if(isNewRound)
                        UIManager.instance.newRound = false;
                    else
                        continue;
                }
                
                RectTransform elementRect = rounds[i][j].GetComponent<RectTransform>();
                Vector2 startPos = elementRect.anchoredPosition;
                Vector2 endPos = new Vector2(startPos.x, startPos.y + UIManager.instance.GetShiftAmount(isNewRound));
                RearrangementElement shiftingElement = new RearrangementElement(startPos, endPos, elementRect);
                newElementList.Add(shiftingElement);
            }
        }

        StartCoroutine(RearrangeElements("SHIFT", newElementList));

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
                healthText.margin = Vector4.Lerp(bounceMargin, defaultMargin, timer);
            else
                healthText.margin = Vector4.Lerp(defaultMargin, bounceMargin, timer);
            
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

    public IEnumerator GoldFlash(TMP_Text goldText)
    {
        float timer = 0f;
        Color startingColor = goldText.color;
        goldText.color = goldFlashColor;

        Vector4 defaultMargin = goldText.margin;
        Vector4 bounceMargin = new Vector4(defaultMargin.x, defaultMargin.y, defaultMargin.z, goldBounceHeight);

        while(timer <= 1f)
        {
            if(timer >= 0.5f)
                goldText.margin = Vector4.Lerp(bounceMargin, defaultMargin, timer);
            else
                goldText.margin = Vector4.Lerp(defaultMargin, bounceMargin, timer);

            timer += Time.deltaTime * goldFlashSpeed;

            if(timer >= 1f)
            {
                goldText.color = startingColor;
                goldText.margin = defaultMargin;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    public IEnumerator StatIncreaseFlash(TMP_Text statText)
    {
        float timer = 0f;

        RectTransform damageTextRect = statText.GetComponent<RectTransform>();
        Vector3 endScale = damageTextRect.localScale;
        damageTextRect.localScale = new Vector3(statIncreaseScale, statIncreaseScale, 1);
        Vector3 startScale = damageTextRect.localScale;

        Color endColor = statText.color;
        statText.color = statFlashColor;

        while(timer <= 1f)
        {
            statText.color = Color.Lerp(statFlashColor, endColor, timer);
            damageTextRect.localScale = Vector3.Lerp(startScale, endScale, timer);

            timer += Time.deltaTime * statFlashSpeedCurve.Evaluate(timer);

            if(timer >= 1f)
            {
                statText.color = endColor;
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
