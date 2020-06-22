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

    [SerializeField] private TMP_Text goldText = default;
    [SerializeField] private GameObject primaryQueueElement = default;
    [SerializeField] private GameObject secondaryQueueElement = default;
    [SerializeField] private GameObject roundText = default;
    [SerializeField] private GameObject deathPanel = default;
    [SerializeField] private GameObject healthImage = default;
    [SerializeField] private TMP_Text damageText = default;
    [SerializeField] private TMP_Text combatPointsText = default;
    public GameObject tooltip;
    [Space]
    [SerializeField] private float queuePanelWidth = default;
    [SerializeField] private float spaceAfterRound = default;
    public int maxCombatQueueElements = default;
    [SerializeField] private float queueElementHeight = default;
    private Transform canvasTransform;
    private Transform queuePanel;
    private RectTransform queuePanelRTransform;
    private Transform playerPanel;
    private Transform healthLayout;
    private Transform healthImages;

    [HideInInspector] public List<List<GameObject>> rounds = new List<List<GameObject>>();
    private int lastRoundVisible = 0;
    public bool newRound = false;

    private float healthElementWidth;
    private EventSystem eventSystem;

    [HideInInspector] public TMP_Text updatingCombatPoints;

    private void Awake()
    {
        instance = this;
        canvasTransform = GameObject.Find("Canvas").transform;
        queuePanel = canvasTransform.Find("CombatQueuePanel");
        queuePanelRTransform = queuePanel.GetComponent<RectTransform>();
        playerPanel = canvasTransform.Find("PlayerUI");

        RectTransform healthRect = healthImage.GetComponent<RectTransform>();
        healthElementWidth = healthRect.rect.width - healthRect.rect.height - 3;

        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    public void InitiateQueueUI(List<GameObject> combatQueue)
    {
        queuePanelRTransform.anchoredPosition = new Vector2(100f, 0f);
        queuePanelRTransform.sizeDelta = new Vector2(100f, 100f);
        AlternativePopulate(combatQueue);

        StartCoroutine(UIAnimations.instance.ShowQueue(queuePanelRTransform));
    }

    public void EndQueueUI()
    {
        StartCoroutine(UIAnimations.instance.HideQueue(queuePanelRTransform));
    }

    public void HealthChange(GameObject combatUnit, int healthUpdate, bool isPlayer, bool heal)
    {
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            int currentRound = CombatManager.instance.currentRound;
            TMP_Text healthText = GetHealthTextFromElement(combatUnit);

            // It always can be that the current round UI doesn't have the damaged unit. We have to check for that.
            if(healthText)
            {
                healthText.text = healthUpdate.ToString();

                // Some juice on the UI is needed as well.
                StartCoroutine(UIAnimations.instance.HealthFlash(healthText));
            }
        }
        
        // Player UI health update.
        int childCount = healthImages.childCount;
        if(isPlayer && !heal)
        {
            int lastIndex = 0;
            for(int i = 0; i < childCount; ++i)
            {
                var uiHealth = healthImages.GetChild(i).GetComponent<UIHealth>();
                if(uiHealth.isEmpty)
                    break;
                lastIndex = i;
            }
            for(int i = lastIndex+1; i > healthUpdate; --i)
            {
                var uiHealth = healthImages.GetChild(i-1).GetComponent<UIHealth>();
                StartCoroutine(uiHealth.Deplete());
            }
        }
        else if(isPlayer && heal)
        {
            int firstIndex = 0;
            for(int i = 0; i < childCount; ++i)
            {
                var uiHealth = healthImages.GetChild(i).GetComponent<UIHealth>();
                firstIndex = i;
                if(uiHealth.isEmpty)
                    break;
            }
            for(int i = firstIndex; i < healthUpdate; ++i)
            {
                var uiHealth = healthImages.GetChild(i).GetComponent<UIHealth>();
                StartCoroutine(uiHealth.Heal());
            }
        }
    }

    public void AlternativeDeathChange(GameObject dyingUnit)
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
                        elementScript.amountToMove += queueElementHeight;
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
    private void AlternativePopulate(List<GameObject> combatQueue)
    {
        // Calculate the total panel height based on how many elements we are drawing.
        float space = spaceAfterRound*2;
        float queuePanelHeight = queueElementHeight * maxCombatQueueElements + space;
        queuePanelRTransform.sizeDelta = new Vector2(queuePanelWidth, queuePanelHeight);

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
                Vector2 newPos = new Vector2(0f, i*queueElementHeight*-1 - spaceAfterRound);
                elementRTransform.anchoredPosition = newPos;
            }
            // Secondary elements
            else
            {
                if(whichRound == 1)
                {
                    rounds.Add(new List<GameObject>());
                    whichRound = 2;
                    Vector2 anchoredPos = new Vector2(0f, i*queueElementHeight*-1 - spaceAfterRound);
                    CreateRoundTextInstance(whichRound, anchoredPos);
                }

                GameObject combatUnit = combatQueue[counter];
                GameObject elementInstance = CreateSecondaryCombatQueueElement(combatUnit, whichRound);

                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(0f, i*queueElementHeight*-1 - space);
                elementRTransform.anchoredPosition = newPos;

                ++counter;
                if(counter == combatQueue.Count)
                {
                    counter = 0;
                    space += spaceAfterRound;
                    queuePanelHeight += spaceAfterRound;
                    queuePanelRTransform.sizeDelta = new Vector2(queuePanelWidth, queuePanelHeight);

                    ++whichRound;

                    if(i != maxCombatQueueElements - 1)
                    {
                        Vector2 anchoredPos = new Vector2(0f, (i+1)*queueElementHeight*-1 - (space - spaceAfterRound)); // what
                        rounds.Add(new List<GameObject>());
                        CreateRoundTextInstance(whichRound, anchoredPos);
                    }
                }
            }
        }
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
        List<GameObject> combatQueue = CombatManager.instance.combatQueue;
        int combatQueueCount = combatQueue.Count;

        int counter = lastRoundElements;
        for(int i = totalQueueElements; i < maxCombatQueueElements; ++i)
        {
            if(i < combatQueueCount)
            {
                GameObject combatUnit = combatQueue[i];
                GameObject elementInstance = CreatePrimaryCombatQueueElement(combatUnit, currentRound);

                // Since the position are not driven by a layout group, we have to do it manually.
                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(200f, i*queueElementHeight*-1 - spaceAfterRound);
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
                    Vector2 anchoredPos = new Vector2(200f, i*queueElementHeight*-1 - spaceAfterRound*GetTotalRoundTexts());
                    RectTransform roundTextRect = CreateRoundTextInstance(lastRoundVisible+1, anchoredPos);
                    RearrangementElement roundTextElement = new RearrangementElement(anchoredPos, new Vector2(0f, anchoredPos.y), roundTextRect);
                    elementList.Add(roundTextElement);
                }
                
                int roundTexts = GetTotalRoundTexts();
                float space = spaceAfterRound * roundTexts;
                GameObject combatUnit = combatQueue[counter];
                GameObject elementInstance = CreateSecondaryCombatQueueElement(combatUnit, lastRoundVisible);


                RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
                Vector2 newPos = new Vector2(200f, i*queueElementHeight*-1 - space);
                elementRTransform.anchoredPosition = newPos;

                RearrangementElement newElement = new RearrangementElement(newPos, new Vector2(0f, newPos.y), elementRTransform);
                elementList.Add(newElement);

                ++counter;
            }
        }

        if(whichStage == "REARRANGING")
            StartCoroutine(UIAnimations.instance.RearrangeElements("NEW", elementList));
        else if(whichStage == "SHIFT")
            StartCoroutine(UIAnimations.instance.RearrangeElements("LAST", elementList));
    }

    private GameObject CreatePrimaryCombatQueueElement(GameObject combatUnit, int whichRound)
    {
        Unit unit = combatUnit.GetComponent<Unit>();

        string unitName = unit.acronym;
        int unitHealth = unit.health;
        int unitCombatPoints = unit.combatPoints;
        Sprite unitImage = unit.combatQueueImage;

        GameObject elementInstance = Instantiate(primaryQueueElement, Vector2.zero, Quaternion.identity, queuePanel);
        rounds[whichRound-1].Add(elementInstance);
        var elementScript = elementInstance.GetComponent<QueueElement>();
        elementScript.attachedGameObject = combatUnit;
        elementScript.whichRound = whichRound;

        Image img = elementInstance.transform.Find("HLayout1").Find("UnitImage").GetComponent<Image>();
        TMP_Text nText = elementInstance.transform.Find("HLayout1").Find("UnitName").GetComponent<TMP_Text>();
        TMP_Text hText = elementInstance.transform.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
        TMP_Text cText = elementInstance.transform.Find("HLayout2").Find("HLayout2").Find("UnitCP").GetComponent<TMP_Text>();

        img.sprite = unitImage;
        nText.text = unitName;
        hText.text = unitHealth.ToString();
        cText.text = unitCombatPoints.ToString() + "/" + unitCombatPoints.ToString();

        return elementInstance;
    }

    private GameObject CreateSecondaryCombatQueueElement(GameObject combatUnit, int whichRound)
    {
        Unit unit = combatUnit.GetComponent<Unit>();

        string unitName = unit.acronym;
        Sprite unitImage = unit.combatQueueImage;

        GameObject elementInstance = Instantiate(secondaryQueueElement, Vector2.zero, Quaternion.identity, queuePanel);
        rounds[whichRound-1].Add(elementInstance);
        var elementScript = elementInstance.GetComponent<QueueElement>();
        elementScript.attachedGameObject = combatUnit;
        elementScript.whichRound = whichRound;

        Image img = elementInstance.transform.Find("UnitImage").GetComponent<Image>();
        TMP_Text nText = elementInstance.transform.Find("UnitName").GetComponent<TMP_Text>();

        img.sprite = unitImage;
        nText.text = unitName;

        return elementInstance;
    }

    private RectTransform CreateRoundTextInstance(int round, Vector2 anchoredPos)
    {
        ++lastRoundVisible;
        GameObject roundTextInstance = Instantiate(roundText, Vector2.zero, Quaternion.identity, queuePanel);
        roundTextInstance.GetComponent<QueueElement>().whichRound = round;
        roundTextInstance.GetComponent<TMP_Text>().text = "Round " + round.ToString();

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
            if(child.name.Contains("RoundText"))
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
            TMP_Text newName = GetNameTextFromElement(newElementScript.attachedGameObject);
            newName.text = unit.acronym;
            TMP_Text newHealth = GetHealthTextFromElement(newElementScript.attachedGameObject);
            newHealth.text = unit.health.ToString();
            TMP_Text newCP = GetCPTextFromElement(newElementScript.attachedGameObject);
            newCP.text = unit.combatPoints.ToString() + "/" + unit.combatPoints.ToString();

            Destroy(oldElementRect.gameObject);
        }
    }

    public void UpdateCombatPoints(int currentCombatPoints, int maxCombatPoints, GameObject combatUnit)
    {
        if(updatingCombatPoints == null)
            updatingCombatPoints = GetCPTextFromElement(combatUnit);
        
        updatingCombatPoints.text = currentCombatPoints + "/" + maxCombatPoints;
    }

    public void InitiatePlayerUI(int health, int combatPoints, int damage)
    {
        healthLayout = playerPanel.Find("HealthLayout");
        healthImages = healthLayout.Find("HealthImages");

        Transform cpLayout = playerPanel.Find("CPLayout");
        Transform dmgLayout = playerPanel.Find("DmgLayout");

        RectTransform healthLayoutRTransform = healthLayout.GetComponent<RectTransform>();
        RectTransform healthTextRTransform = healthLayout.Find("HealthText").GetComponent<RectTransform>();
        
        for(int i = 0; i < health; ++i)
        {
            GameObject go = Instantiate(healthImage, Vector2.zero, Quaternion.identity, healthImages.transform);
            RectTransform goRect = go.GetComponent<RectTransform>();

            Vector2 newPos = new Vector2(healthLayoutRTransform.anchoredPosition.x + (i * healthElementWidth) + healthTextRTransform.rect.width + 30, healthLayoutRTransform.anchoredPosition.y + healthLayoutRTransform.rect.height / 2);
            goRect.anchoredPosition = newPos;
        }

        cpLayout.Find("CPText").GetComponent<TMP_Text>().text = combatPoints.ToString();
        dmgLayout.Find("DmgText").GetComponent<TMP_Text>().text = damage.ToString();
    }

    // Looks simple, isn't.
    public void IncreasePlayerHealth(int amount)
    {
        healthLayout = playerPanel.Find("HealthLayout");
        healthImages = healthLayout.Find("HealthImages");

        RectTransform healthLayoutRTransform = healthLayout.GetComponent<RectTransform>();
        RectTransform healthTextRTransform = healthLayout.Find("HealthText").GetComponent<RectTransform>();

        // Find the first empty health image on the UI. If there is none, set the index to the last image.
        int healthIndex = 0;
        for(int i = 0; i < healthImages.childCount; ++i)
        {
            var healthComponent = healthImages.GetChild(i).GetComponent<UIHealth>();
            if(healthComponent.isEmpty)
            {
                healthIndex = i;
                break;
            }

            if(i == healthImages.childCount - 1)
                healthIndex = healthImages.childCount;
        }

        // Move empty images if there are any to make room.
        for(int i = healthIndex; i < healthImages.childCount; ++i)
        {
            Transform healthChild = healthImages.GetChild(i);
            RectTransform healthRect = healthChild.GetComponent<RectTransform>();

            Vector2 newPos = new Vector2(healthRect.anchoredPosition.x + healthElementWidth, healthRect.anchoredPosition.y);
            healthRect.anchoredPosition = newPos;
        }

        // Create a new health image and insert it into the empty space.
        int offsetIndex = healthIndex - 1;
        int counter = 1;
        for(int i = healthIndex; i < healthIndex + amount; ++i)
        {
            RectTransform offsetRect = healthImages.GetChild(offsetIndex).GetComponent<RectTransform>();

            GameObject healthGo = Instantiate(healthImage, Vector2.zero, Quaternion.identity, healthImages);
            RectTransform healthGoRect = healthGo.GetComponent<RectTransform>();
            healthGo.transform.SetSiblingIndex(i);

            Vector2 newHealthPos = new Vector2(offsetRect.anchoredPosition.x + (healthElementWidth*counter), offsetRect.anchoredPosition.y);
            healthGoRect.anchoredPosition = newHealthPos;

            StartCoroutine(healthGo.GetComponent<UIHealth>().Increase());
            ++counter;
        }
    }

    public void UpdateGoldText(int gold)
    {
        string toUpdate = "GOLD: " + gold.ToString();
        goldText.text = toUpdate;
        StartCoroutine(UIAnimations.instance.GoldFlash(goldText));
    }

    public void IncreaseDamageText(int amount)
    {
        damageText.text = amount.ToString();
        StartCoroutine(UIAnimations.instance.StatIncreaseFlash(damageText));
    }

    public void IncreaseCombatPointsText(int amount)
    {
        combatPointsText.text = amount.ToString();
        StartCoroutine(UIAnimations.instance.StatIncreaseFlash(combatPointsText));
    }

    private TMP_Text GetCPTextFromElement(GameObject combatUnit)
    {
        int currentRound = CombatManager.instance.currentRound;
        for(int i = 0; i < rounds[currentRound-1].Count; ++i)
        {
            var elementScript = rounds[currentRound-1][i].GetComponent<QueueElement>();
            if(elementScript.attachedGameObject == combatUnit)
                return rounds[currentRound-1][i].transform.Find("HLayout2").Find("HLayout2").Find("UnitCP").GetComponent<TMP_Text>();
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
                return rounds[currentRound-1][i].transform.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
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
                return rounds[currentRound-1][i].transform.Find("HLayout1").Find("UnitName").GetComponent<TMP_Text>();
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
                return rounds[currentRound-1][i].transform.Find("HLayout1").Find("UnitImage").GetComponent<Image>();
        }

        return null;
    }

    public void ClearCombatQueue()
    {
        for(int i = 0; i < rounds.Count; ++i)
            for(int j = 0; j < rounds.Count; ++j)
                Destroy(rounds[i][j]);
        rounds.Clear();
        
    }

    public void DeselectButton()
    {
        eventSystem.SetSelectedGameObject(null);
    }

    // temporary (or not)
    public float GetShiftAmount(bool isNewRound)
    {
        if(isNewRound)
            return queuePanelWidth + spaceAfterRound;
        else
            return queuePanelWidth;
    }
}
