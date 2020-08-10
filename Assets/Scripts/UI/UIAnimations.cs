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

    [Header("UI Bar animation")]
    [SerializeField] private float healthDepleteSpeed = default;
    [SerializeField] private float healthBufferPeriod = default;
    [SerializeField] private Color healthDepleteFlashColor = default;
    [SerializeField] private float cpDepleteSpeed = default;
    [SerializeField] private float cpBufferPeriod = default;
    [SerializeField] private Color cpDepleteFlashColor = default;
    private Image healthDepleteBar;
    private Image healthBar;
    private Image cpDepleteBar;
    private Image cpBar;
    private float healthDepleteTimer = 0f;
    [HideInInspector] public bool isHealthDepleteTimerRunning = false;
    private float healthDepleteColorLerp = 0f;
    private float cpDepleteTimer = 0f;
    [HideInInspector] public bool isCPDepleteTimerRunning = false;
    private float cpDepleteColorLerp = 0f;

    [Header("Gold flash animation")]
    public Vector2 goldTextOffset;
    public float goldTextFadeRate;
    [SerializeField] private Vector2 goldDistortion = default;
    [SerializeField] private float goldJump = default;
    [SerializeField] private float goldJumpRate = default;
    [SerializeField] private float goldFallRate = default;
    private float goldFlashTimer = 0f;
    private int goldFlashPhase = 1;
    private bool isGoldFlashRunning = false;

    [HideInInspector] public List<RearrangementElement> secondElementList = new List<RearrangementElement>();

    private void Awake()
    {
        instance = this;
        healthDepleteBar = UIManager.instance.healthFillFlash;
        cpDepleteBar = UIManager.instance.cpFillFlash;
        healthBar = UIManager.instance.healthFill;
        cpBar = UIManager.instance.cpFill;
    }

    private void Update()
    {
        HealthDepleteTimer();
        CPDepleteTimer();
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

                Transform queuePanel = elementRect.transform.Find("CombatQueuePanel");

                for(int i = 0; i < queuePanel.childCount; ++i)
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
        if(!GameStateManager.instance.CheckState("COMBAT"))
            yield break;

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

    public void StartHealthDepleteTimer()
    {
        if(isHealthDepleteTimerRunning)
            healthDepleteTimer = 0f;
        else
        {
            healthDepleteBar.color = healthDepleteFlashColor;
            isHealthDepleteTimerRunning = true;
        }
    }

    public void StartCPDepleteTimer()
    {
        if(isCPDepleteTimerRunning)
            cpDepleteTimer = 0f;
        else
        {
            cpDepleteBar.color = cpDepleteFlashColor;
            isCPDepleteTimerRunning = true;
        }
    }

    private void HealthDepleteTimer()
    {
        if(isHealthDepleteTimerRunning)
        {
            healthDepleteTimer += Time.deltaTime;
            healthDepleteBar.color = Color.Lerp(healthDepleteFlashColor, Color.white, healthDepleteTimer/healthBufferPeriod);
            if(healthDepleteTimer >= healthBufferPeriod)
            {
                healthDepleteTimer = 0f;
                isHealthDepleteTimerRunning = false;
                healthDepleteBar.color = Color.white;
                StartCoroutine(HealthDeplete());
            }
        }
    }

    private void CPDepleteTimer()
    {
        if(isCPDepleteTimerRunning)
        {
            cpDepleteTimer += Time.deltaTime;
            cpDepleteBar.color = Color.Lerp(cpDepleteFlashColor, Color.white, cpDepleteTimer/cpBufferPeriod);
            if(cpDepleteTimer >= cpBufferPeriod)
            {
                cpDepleteTimer = 0f;
                isCPDepleteTimerRunning = false;
                cpDepleteBar.color = Color.white;
                StartCoroutine(CPDeplete());
            }
        }
    }

    private IEnumerator HealthDeplete()
    {
        float timer = healthDepleteBar.fillAmount;
        float endTime = healthBar.fillAmount;
        
        while(timer >= endTime)
        {
            timer -= Time.deltaTime * healthDepleteSpeed;
            healthDepleteBar.fillAmount = timer;

            if(timer <= endTime)
            {
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    private IEnumerator CPDeplete()
    {
        float timer = cpDepleteBar.fillAmount;
        float endTime = cpBar.fillAmount;

        while(timer >= endTime)
        {
            timer -= Time.deltaTime * cpDepleteSpeed;
            cpDepleteBar.fillAmount = timer;

            if(timer <= 0f)
            {
                yield break;     
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    public void GoldFlash(Transform goldText, List<TMP_Text> texts)
    {
        if(!isGoldFlashRunning)
        {
            isGoldFlashRunning = true;
            StartCoroutine(GoldFlashCoroutine(goldText, texts));
        }
        else
        {
            goldFlashPhase = 1;
            goldFlashTimer = 0f;
        }
    }

    private IEnumerator GoldFlashCoroutine(Transform goldText, List<TMP_Text> texts)
    {
        foreach(TMP_Text txt in texts)
        {
            txt.color = Color.white;
            txt.fontSize = 80f;
        }

        goldFlashTimer = 0f;
        goldFlashPhase = 1;
        
        var rect = goldText.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + goldJump);

        float rate = goldJumpRate;

        while(goldFlashTimer <= 1f)
        {
            goldFlashTimer += Time.deltaTime * rate;

            if(goldFlashPhase == 1)
            {
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, goldFlashTimer);
                goldText.localScale = Vector2.Lerp(Vector2.one, goldDistortion, goldFlashTimer);
            }
            else
            {
                rect.anchoredPosition = Vector2.Lerp(endPos, startPos, goldFlashTimer);
                goldText.localScale = Vector2.Lerp(goldDistortion, Vector2.one, goldFlashTimer);
            }

            if(goldFlashTimer >= 1f && goldFlashPhase == 1)
            {
                foreach(TMP_Text txt in texts)
                {
                    if(!txt.name.Contains("White"))
                        txt.color = Color.black;
                    txt.fontSize = 50f;
                }
                
                ++goldFlashPhase;
                goldFlashTimer = 0f;
                rate = goldFallRate;
            }
            else if(goldFlashTimer >= 1f && goldFlashPhase != 1)
            {   
                rect.anchoredPosition = startPos;
                isGoldFlashRunning = false;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }

        }

        yield break;
    }

}
