using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private GameObject primaryQueueElement = default;
    [SerializeField] private GameObject secondaryQueueElement = default;
    [SerializeField] private GameObject roundText = default;
    [SerializeField] private GameObject deathPanel = default;
    [SerializeField] private GameObject goldFlash = default;
    [SerializeField] private Color cpDisabled = default;
    [SerializeField] private Color cpEnabled = default;
    [Space]
    [SerializeField] private float spaceAfterRound = default;
    [Space]
    public AnimationCurve statFadeCurve;
    public Color statAdditionColor;

    private Transform playerUI;
    public int maxCombatQueueElements = default;
    private Transform queuePanel;
    private RectTransform queuePanelRTransform;
    private RectTransform combatVisRect;
    [HideInInspector] public Button skipButton;
    [HideInInspector] public Button waitButton;
    private Vector2 queueElementSize;
    private float combatVisualisationBaseHeight;
    [Space]
    [HideInInspector] public Image healthFill;
    [HideInInspector] public Image healthFillFlash;
    private float healthFillWidth;
    private Transform healthTextMain;
    [HideInInspector] public Image cpFill;
    [HideInInspector] public Image cpFillFlash;
    private Transform cpTextMain;
    private float cpFillWidth;
    private Unit playerUnit;
    private Transform goldTextObject;
    private List<TMP_Text> goldTextBunch = new List<TMP_Text>();

    // Stats
    private Dictionary<string, TMP_Text> statDict = new Dictionary<string, TMP_Text>();

    private int lastRoundVisible = 0;
    [HideInInspector] public List<List<GameObject>> rounds = new List<List<GameObject>>();
    [HideInInspector] public bool newRound = false;
    [HideInInspector] public TMP_Text updatingCombatPoints;
    private EventSystem eventSystem;

    private void Awake()
    {
        instance = this;
        Screen.SetResolution(1920, 1080, true, 60);

        Transform canvasTransform = GameObject.Find("Canvas").transform;
        Transform combatVisualisation = canvasTransform.Find("CombatVisualisation");
        combatVisRect = combatVisualisation.GetComponent<RectTransform>();
        combatVisualisationBaseHeight = combatVisRect.sizeDelta.y;
        queuePanel = combatVisualisation.Find("CombatQueuePanel");
        queuePanelRTransform = queuePanel.GetComponent<RectTransform>();
        Transform playerPanel = canvasTransform.Find("PlayerUI");
        skipButton = combatVisualisation.Find("CombatActionsPanel").Find("SkipButton").GetComponent<Button>();
        waitButton = combatVisualisation.Find("CombatActionsPanel").Find("WaitButton").GetComponent<Button>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        queueElementSize = primaryQueueElement.GetComponent<RectTransform>().sizeDelta;

        playerUnit = GameObject.Find("Player").GetComponent<Unit>();
        playerUI = canvasTransform.Find("PlayerUI");
        Transform hContainer = playerUI.Find("HealthBar");
        Transform cpContainer = playerUI.Find("CombatPointsBar");
        healthFill = playerUI.Find("HealthFill").GetComponent<Image>();
        cpFill = playerUI.Find("CombatPointsFill").GetComponent<Image>();
        healthFillFlash = playerUI.Find("HealthFlash").GetComponent<Image>();
        cpFillFlash = playerUI.Find("CombatPointsFlash").GetComponent<Image>();
        healthFillWidth = playerUI.Find("HealthFill").GetComponent<RectTransform>().sizeDelta.x;
        cpFillWidth = playerUI.Find("CombatPointsFill").GetComponent<RectTransform>().sizeDelta.x;

        healthTextMain = hContainer.Find("HealthTextMain");
        cpTextMain = cpContainer.Find("CombatPointsTextMain");
        string healthString = playerUnit.health.ToString() + "/" + playerUnit.maxHealth.ToString();
        string cpString = playerUnit.currentCombatPoints.ToString() + "/" + playerUnit.combatPoints.ToString();
        UpdateText(healthTextMain, healthString);
        UpdateText(cpTextMain, cpString);

        goldTextObject = playerUI.Find("Gold").Find("GoldTextMain");
        goldTextBunch.Add(goldTextObject.GetComponent<TMP_Text>());
        for(int i = 0; i < goldTextObject.childCount; ++i)
            goldTextBunch.Add(goldTextObject.GetChild(i).GetComponent<TMP_Text>());

        TMP_Text damageText = playerUI.Find("Damage").Find("DamageText").GetComponent<TMP_Text>();
        damageText.text = playerUnit.damage.ToString();
        statDict.Add("Damage", damageText);
    }

    private void UpdateText(Transform textObject, string text)
    {
        textObject.GetComponent<TMP_Text>().text = text;
        for(int i = 0; i < textObject.childCount; ++i)
            textObject.GetChild(i).GetComponent<TMP_Text>().text = text;
    }

    public void InitiateQueueUI(List<GameObject> combatQueue)
    {
        queuePanelRTransform.sizeDelta = queueElementSize;
        cpFill.color = cpEnabled;
        Populate(combatQueue);

        StartCoroutine(UIAnimations.instance.ShowQueue(combatVisRect));
    }

    public void EndQueueUI()
    {
        cpFill.color = cpDisabled;
        playerUnit.currentCombatPoints = playerUnit.combatPoints;
        cpFill.fillAmount = 1f;
        StartCoroutine(UIAnimations.instance.HideQueue(combatVisRect));
    }

    public void HealthChange(GameObject combatUnit, int previousHealth, int currentHealth, bool isPlayer, bool heal)
    {
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            int currentRound = CombatManager.instance.currentRound;
            TMP_Text healthText = GetHealthTextFromElement(combatUnit);

            // It always can be that the current round UI doesn't have the damaged unit. We have to check for that.
            if(healthText)
            {
                healthText.text = previousHealth.ToString();
                healthText.transform.GetChild(0).GetComponent<TMP_Text>().text = previousHealth.ToString();

                // Some juice on the UI is needed as well.
                StartCoroutine(UIAnimations.instance.HealthFlash(healthText));
            }
        }
        
        // Player UI health update.
        if(combatUnit.CompareTag("Player"))
        {
            float currentFill = healthFill.fillAmount;
            float currentFillRaw = currentFill * healthFillWidth;
            float neededFill = (currentFill*currentHealth)/previousHealth;
            healthFill.fillAmount = neededFill;

            string healthString = currentHealth.ToString() + "/" + playerUnit.maxHealth.ToString();
            UpdateText(healthTextMain, healthString);

            UIAnimations.instance.StartHealthDepleteTimer();
        }
        
    }

    public void DeathChange(GameObject dyingUnit)
    {
        List<RearrangementElement> elementList = new List<RearrangementElement>();
        List<RectTransform> deathElements = new List<RectTransform>();

        // Cycle through each round and each round element, gather up dying elements as well as other moving elements.
        int currentRound = CombatManager.instance.currentRound;
        for(int i = currentRound-1; i < rounds.Count; ++i)
        {
            List<int> indices = new List<int>();
            for(int j = 0; j < rounds[i].Count; ++j)
            {
                GameObject element = rounds[i][j];
                var elementScript = element.GetComponent<QueueElement>();

                if(elementScript.attachedGameObject == dyingUnit)
                {
                    // Getting the dying unit index in the combat queue. Everything below should be moved above.
                    indices.Add(j);

                    // Creating the skull on the dying unit's element.
                    GameObject skull = Instantiate(deathPanel, Vector2.zero, Quaternion.identity, element.transform);
                    RectTransform skullRect = skull.GetComponent<RectTransform>();
                    skullRect.anchoredPosition = Vector2.zero;

                    // Setting up values to move the element away
                    RectTransform elementRect = element.GetComponent<RectTransform>();
                    deathElements.Add(elementRect);
                    Vector2 newPos = new Vector2(elementRect.rect.width*4, elementRect.anchoredPosition.y);
                    RearrangementElement movingElement = new RearrangementElement(elementRect.anchoredPosition, newPos, elementRect);

                    rounds[i].RemoveAt(j);
                    elementList.Add(movingElement);
                }
            }
        }

        // Triple layered loop to gather amount to move for each element in the rounds.
        foreach(RectTransform deathElement in deathElements)
        {
            for(int i = currentRound-1; i < rounds.Count; ++i)
            {
                for(int j = 0; j < rounds[i].Count; ++j)
                {
                    RectTransform element = rounds[i][j].GetComponent<RectTransform>();
                    var elementScript = element.GetComponent<QueueElement>();

                    if(element.anchoredPosition.y < deathElement.anchoredPosition.y)
                        elementScript.amountToMove += queueElementSize.y;
                }
            }
        }

        // Go through each element
        for(int i = currentRound-1; i < rounds.Count; ++i)
        {
            for(int j = 0; j < rounds[i].Count; ++j)
            {
                RectTransform element = rounds[i][j].GetComponent<RectTransform>();
                var elementScript = element.GetComponent<QueueElement>();

                if(elementScript.attachedGameObject == dyingUnit)
                    continue;

                Vector2 startPos = element.anchoredPosition;
                Vector2 endPos = new Vector2(element.anchoredPosition.x, element.anchoredPosition.y + elementScript.amountToMove);
                elementScript.amountToMove = 0f;
                RearrangementElement rElement = new RearrangementElement(startPos, endPos, element);
                UIAnimations.instance.secondElementList.Add(rElement);
            }
        }

        // Moving the elements.
        StartCoroutine(UIAnimations.instance.RearrangeElements("DYING", elementList));
    }

    // Trying to expand the combat queue to show multiple rounds...
    private void Populate(List<GameObject> combatQueue)
    {
        // Calculate the total panel height based on how many elements we are drawing.
        float space = spaceAfterRound*2;
        float queuePanelHeight = queueElementSize.y * maxCombatQueueElements + space;
        queuePanelRTransform.sizeDelta = new Vector2(queueElementSize.x, queuePanelHeight);

        int counter = 0;
        int whichRound = 1;
        for(int i = 0; i < maxCombatQueueElements; ++i)
        {
            // Primary elements
            if(i < combatQueue.Count)
            {
                if(i == 0)
                {
                    rounds.Add(new List<GameObject>());
                    CreateRoundTextInstance(whichRound, Vector2.zero);
                }

                GameObject combatUnit = combatQueue[i];
                GameObject elementInstance = CreatePrimaryCombatQueueElement(combatUnit, whichRound);

                // Since the position are not driven by a layout group, we have to do it manually.
                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(0f, i*queueElementSize.y*-1 - spaceAfterRound);
                elementRTransform.anchoredPosition = newPos;
            }
            // Secondary elements
            else
            {
                if(whichRound == 1)
                {
                    rounds.Add(new List<GameObject>());
                    whichRound = 2;
                    Vector2 anchoredPos = new Vector2(0f, i*queueElementSize.y*-1 - spaceAfterRound);
                    CreateRoundTextInstance(whichRound, anchoredPos);
                }

                GameObject combatUnit = combatQueue[counter];
                GameObject elementInstance = CreateSecondaryCombatQueueElement(combatUnit, whichRound);

                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(0f, i*queueElementSize.y*-1 - space);
                elementRTransform.anchoredPosition = newPos;

                ++counter;
                if(counter == combatQueue.Count)
                {
                    counter = 0;
                    space += spaceAfterRound;
                    queuePanelHeight += spaceAfterRound;
                    queuePanelRTransform.sizeDelta = new Vector2(queueElementSize.x, queuePanelHeight);

                    ++whichRound;

                    if(i != maxCombatQueueElements - 1)
                    {
                        Vector2 anchoredPos = new Vector2(0f, (i+1)*queueElementSize.y*-1 - (space - spaceAfterRound)); // what
                        rounds.Add(new List<GameObject>());
                        CreateRoundTextInstance(whichRound, anchoredPos);
                    }
                }
            }
        }
        combatVisRect.sizeDelta = new Vector2(combatVisRect.sizeDelta.x, combatVisualisationBaseHeight + queuePanelRTransform.sizeDelta.y);
    }

    public void RefillQueue(int lastRoundElements, string whichStage)
    {
        List<RearrangementElement> elementList = new List<RearrangementElement>();

        int totalQueueElements = 0;
        int currentRound = CombatManager.instance.currentRound;
        for(int i = currentRound-1; i < rounds.Count; ++i)
            for(int j = 1; j < rounds[i].Count; ++j)
                ++totalQueueElements;

        int refillCount = maxCombatQueueElements - totalQueueElements;
        List<GameObject> combatOrder = CombatManager.instance.combatOrder;
        int combatQueueCount = combatOrder.Count;

        int counter = lastRoundElements;
        for(int i = totalQueueElements; i < maxCombatQueueElements; ++i)
        {
            if(i < combatQueueCount)
            {
                GameObject combatUnit = combatOrder[i];
                GameObject elementInstance = CreatePrimaryCombatQueueElement(combatUnit, currentRound);

                // Since the position are not driven by a layout group, we have to do it manually.
                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(200f, i*queueElementSize.y*-1 - spaceAfterRound);
                elementRTransform.anchoredPosition = newPos;

                RearrangementElement newElement = new RearrangementElement(newPos, new Vector2(0f, newPos.y), elementRTransform);
                elementList.Add(newElement);
            }
            else
            {
                if(counter == combatQueueCount)
                {
                    counter = 0;
                    rounds.Add(new List<GameObject>());
                    Vector2 anchoredPos = new Vector2(200f, i*queueElementSize.y*-1 - spaceAfterRound*GetTotalRoundTexts());
                    RectTransform roundTextRect = CreateRoundTextInstance(lastRoundVisible+1, anchoredPos);
                    RearrangementElement roundTextElement = new RearrangementElement(anchoredPos, new Vector2(0f, anchoredPos.y), roundTextRect);
                    elementList.Add(roundTextElement);
                }
                
                int roundTexts = GetTotalRoundTexts();
                float space = spaceAfterRound * roundTexts;
                GameObject combatUnit = combatOrder[counter];
                GameObject elementInstance = CreateSecondaryCombatQueueElement(combatUnit, lastRoundVisible);

                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(200f, i*queueElementSize.y*-1 - space);
                elementRTransform.anchoredPosition = newPos;

                RearrangementElement newElement = new RearrangementElement(newPos, new Vector2(0f, newPos.y), elementRTransform);
                elementList.Add(newElement);

                ++counter;
            }
        }

        combatVisRect.sizeDelta = new Vector2(combatVisRect.sizeDelta.x, combatVisualisationBaseHeight + queuePanelRTransform.sizeDelta.y);

        if(whichStage == "REARRANGING")
            StartCoroutine(UIAnimations.instance.RearrangeElements("NEW", elementList));
        else if(whichStage == "SHIFT")
            StartCoroutine(UIAnimations.instance.RearrangeElements("LAST", elementList));
    }

    private GameObject CreatePrimaryCombatQueueElement(GameObject combatUnit, int whichRound)
    {
        Unit unit = combatUnit.GetComponent<Unit>();

        string unitName = unit.name;
        int unitHealth = unit.health;
        int unitCombatPoints = unit.combatPoints;
        Sprite unitImage = unit.combatQueueImage;

        GameObject elementInstance = Instantiate(primaryQueueElement, Vector2.zero, Quaternion.identity, queuePanel);
        rounds[whichRound-1].Add(elementInstance);
        var elementScript = elementInstance.GetComponent<QueueElement>();
        elementScript.attachedGameObject = combatUnit;
        elementScript.whichRound = whichRound;

        Image img = elementInstance.transform.Find("UnitImage").GetComponent<Image>();
        img.sprite = unitImage;

        Transform nameTextObject = elementInstance.transform.Find("UnitNameMain");
        UpdateText(nameTextObject, unitName.ToUpper());

        Transform healthTextObject = elementInstance.transform.Find("UnitHealthMain");
        UpdateText(healthTextObject, unitHealth.ToString());

        Transform cpTextObject = elementInstance.transform.Find("UnitCPMain");
        UpdateText(cpTextObject, unitCombatPoints.ToString());

        return elementInstance;
    }

    private GameObject CreateSecondaryCombatQueueElement(GameObject combatUnit, int whichRound)
    {
        Unit unit = combatUnit.GetComponent<Unit>();

        string unitName = unit.name;
        Sprite unitImage = unit.combatQueueImage;

        GameObject elementInstance = Instantiate(secondaryQueueElement, Vector2.zero, Quaternion.identity, queuePanel);
        rounds[whichRound-1].Add(elementInstance);
        var elementScript = elementInstance.GetComponent<QueueElement>();
        elementScript.attachedGameObject = combatUnit;
        elementScript.whichRound = whichRound;

        Image img = elementInstance.transform.Find("UnitImage").GetComponent<Image>();
        img.sprite = unitImage;

        Transform nameTextObject = elementInstance.transform.Find("UnitNameMain");
        UpdateText(nameTextObject, unitName.ToUpper());

        return elementInstance;
    }

    private RectTransform CreateRoundTextInstance(int round, Vector2 anchoredPos)
    {
        ++lastRoundVisible;
        GameObject roundTextInstance = Instantiate(roundText, Vector2.zero, Quaternion.identity, queuePanel);
        roundTextInstance.GetComponent<QueueElement>().whichRound = round;

        string roundTextString = "ROUND " + round.ToString();
        UpdateText(roundTextInstance.transform, roundTextString);

        RectTransform instanceRect = roundTextInstance.GetComponent<RectTransform>();
        instanceRect.anchoredPosition = anchoredPos;

        rounds[round-1].Add(roundTextInstance);

        return instanceRect;
    }

    private int GetTotalRoundTexts()
    {
        int roundTexts = 0;
        for(int j = 0; j < queuePanel.childCount; ++j)
        {
            GameObject child = queuePanel.GetChild(j).gameObject;
            if(child.name.Contains("RoundTextMain"))
                roundTexts++;
        }
        return roundTexts;
    }

    public void HideElement()
    {
        int currentRound = CombatManager.instance.currentRound;
        List<RearrangementElement> elementList = new List<RearrangementElement>();
        GameObject element = rounds[currentRound-1][1];
        RectTransform elementRect = element.GetComponent<RectTransform>();
        Vector2 startPos = elementRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x + 200f, startPos.y);
        RearrangementElement rElement = new RearrangementElement(startPos, endPos, elementRect);
        elementList.Add(rElement);
        rounds[currentRound-1].RemoveAt(1);
        StartCoroutine(UIAnimations.instance.RearrangeElements("HIDE", elementList));
    }

    public void NextRound()
    {
        ResetCombatPointsBar();

        int currentRound = CombatManager.instance.currentRound;
        List<RearrangementElement> elementList = new List<RearrangementElement>();
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            RectTransform elementRect = rounds[currentRound-1][i].GetComponent<RectTransform>();
            Vector2 startPos = elementRect.anchoredPosition;
            Vector2 endPos = new Vector2(startPos.x + 200f, startPos.y);
            RearrangementElement rElement = new RearrangementElement(startPos, endPos, elementRect);
            elementList.Add(rElement);
        }

        ++CombatManager.instance.currentRound;
        ConvertElements();

        newRound = true;
        StartCoroutine(UIAnimations.instance.RearrangeElements("HIDE", elementList));
    }

    public void ConvertElements()
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 1; i < rounds[currentRound-1].Count; ++i)
        {
            RectTransform oldElementRect = rounds[currentRound-1][i].GetComponent<RectTransform>();
            var oldElementScript = oldElementRect.GetComponent<QueueElement>();

            RectTransform newElementRect = Instantiate(primaryQueueElement, Vector2.zero, Quaternion.identity, queuePanel).GetComponent<RectTransform>();
            newElementRect.anchoredPosition = oldElementRect.anchoredPosition;
            var newElementScript = newElementRect.GetComponent<QueueElement>();
            rounds[currentRound-1][i] = newElementRect.gameObject;

            newElementScript.whichRound = oldElementScript.whichRound;
            newElementScript.attachedGameObject = oldElementScript.attachedGameObject;
            newElementScript.amountToMove = 0f;

            Unit unit = newElementScript.attachedGameObject.GetComponent<Unit>();

            Image newImg = GetImageFromElement(newElementScript.attachedGameObject);
            newImg.sprite = unit.combatQueueImage;

            Transform newNameMain = GetNameTextFromElement(newElementScript.attachedGameObject).transform;
            UpdateText(newNameMain, unit.name.ToUpper());

            Transform newHealthMain = GetHealthTextFromElement(newElementScript.attachedGameObject).transform;
            UpdateText(newHealthMain, unit.health.ToString());

            Transform newCPMain = GetCPTextFromElement(newElementScript.attachedGameObject).transform;
            UpdateText(newCPMain, unit.combatPoints.ToString());

            Destroy(oldElementRect.gameObject);
        }
    }

    public void UpdateCombatPoints(int previousCombatPoints, int currentCombatPoints, int maxCombatPoints, GameObject combatUnit)
    {
        updatingCombatPoints = GetCPTextFromElement(combatUnit);

        // Updating the combat queue element combat points.
        UpdateText(updatingCombatPoints.transform, currentCombatPoints.ToString());

        if(combatUnit.CompareTag("Player"))
        {
            float currentFill = cpFill.fillAmount;
            float neededFill = (float)currentCombatPoints/(float)maxCombatPoints;
            cpFill.fillAmount = neededFill;

            string cpString = currentCombatPoints + "/" + maxCombatPoints;
            UpdateText(cpTextMain, cpString);
            UIAnimations.instance.StartCPDepleteTimer();
        }
    }

    public void IncreasePlayerHealth(int amount)
    {
        
    }

    public void UpdateGoldText(int gold, string goldChange)
    {
        string goldText = gold.ToString();
        UpdateText(goldTextObject, goldText);
        Transform goldFlashInstance = Instantiate(goldFlash, playerUI).transform;
        UpdateText(goldFlashInstance, goldChange);
        UIAnimations.instance.GoldFlash(goldTextObject, goldTextBunch);
    }

    private TMP_Text GetCPTextFromElement(GameObject combatUnit)
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            if(elementScript.attachedGameObject == combatUnit)
                return rounds[currentRound-1][i].transform.Find("UnitCPMain").GetComponent<TMP_Text>();
        }
        return null;
    }

    private TMP_Text GetHealthTextFromElement(GameObject combatUnit)
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            if(elementScript.attachedGameObject == combatUnit)
                return rounds[currentRound-1][i].transform.Find("UnitHealthMain").GetComponent<TMP_Text>();
        }

        return null;
    }

    private TMP_Text GetNameTextFromElement(GameObject combatUnit)
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            if(elementScript.attachedGameObject == combatUnit)
                return rounds[currentRound-1][i].transform.Find("UnitNameMain").GetComponent<TMP_Text>();
        }

        return null;
    }

    private Image GetImageFromElement(GameObject combatUnit)
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            if(elementScript.attachedGameObject == combatUnit)
                return rounds[currentRound-1][i].transform.Find("UnitImage").GetComponent<Image>();
        }

        return null;
    }

    public void ClearCombatQueue()
    {
        for(int i = 0; i < queuePanel.childCount; ++i)
        {
            Destroy(queuePanel.GetChild(i).gameObject);
        }
        rounds.Clear();
    }

    public void DeselectButton()
    {
        eventSystem.SetSelectedGameObject(null);
    }

    public void WaitOnClick()
    {
        DeselectButton();
        waitButton.interactable = false;
        skipButton.interactable = false;

        if(!GameStateManager.instance.CheckState("COMBAT"))
            return;

        List<RearrangementElement> elementList = new List<RearrangementElement>();
        string whoseTurn = CombatManager.instance.whoseTurn;
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 1; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            var rect = rounds[currentRound-1][i].GetComponent<RectTransform>();
            if(elementScript.attachedGameObject.name == whoseTurn)
            {
                Vector2 newPos = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - (rounds[currentRound-1].Count-2)*queueElementSize.y);
                RearrangementElement newElement = new RearrangementElement(rect.anchoredPosition, newPos, rect);
                elementList.Insert(0, newElement);
            }
            else
            {
                Vector2 newPos = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + queueElementSize.y);
                RearrangementElement newElement = new RearrangementElement(rect.anchoredPosition, newPos, rect);
                elementList.Add(newElement);
            }
        }

        CombatManager.instance.movementTilemap.ClearAllTiles();
        StartCoroutine(UIAnimations.instance.RearrangeElements("WAIT", elementList));
    }

    public void SkipOnClick()
    {
        DeselectButton();

        if(!GameStateManager.instance.CheckState("COMBAT"))
            return;
        
        CombatManager.instance.movementTilemap.ClearAllTiles();
        CombatManager.instance.NextTurn();
    }

    public void PushDownElement(int index)
    {
        // Remove the unit from the queue and re-add it so that it drops down the queue.
        int currentRound = CombatManager.instance.currentRound;
        var tmp = rounds[currentRound-1][index];
        rounds[currentRound-1].RemoveAt(index);
        rounds[currentRound-1].Add(tmp);
    }

    // temporary (or not)
    public float GetShiftAmount(bool isNewRound)
    {
        if(isNewRound)
            return queueElementSize.x + spaceAfterRound;
        else
            return queueElementSize.x;
    }

    public void ResetCombatPointsBar()
    {
        cpFill.fillAmount = 1;
        cpFillFlash.fillAmount = 1;

        string cpString = playerUnit.combatPoints.ToString() + "/" + playerUnit.combatPoints.ToString();
        UpdateText(cpTextMain, cpString);
    }

    public void IncreaseStat(string whichStat, int amount)
    {
        playerUnit.damage += amount;
        statDict[whichStat].text = (amount + playerUnit.damage).ToString();
        Transform increaseText = statDict[whichStat].transform.Find("StatAddition");
        increaseText.gameObject.SetActive(true);
        increaseText.GetComponent<StatIncrease>().Enable(amount);
    }
}
