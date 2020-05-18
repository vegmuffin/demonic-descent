using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private TMP_Text goldText = default;
    [SerializeField] private GameObject queueElement = default;
    [SerializeField] private GameObject deathPanel = default;
    [SerializeField] private GameObject healthImage = default;
    [SerializeField] private TMP_Text damageText = default;
    [SerializeField] private TMP_Text combatPointsText = default;
    [SerializeField] private float queuePanelWidth = default;
    [Space]
    public GameObject tooltip;

    private Transform canvasTransform;
    private Transform queuePanel;
    private RectTransform queuePanelRTransform;
    private List<GameObject> panelElements = new List<GameObject>();
    private Transform playerPanel;
    private Transform healthLayout;
    private Transform healthImages;

    private float healthElementWidth;

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
    }

    public void InitiateQueueUI(List<GameObject> combatQueue)
    {
        PopulateQueueUI(combatQueue);

        StartCoroutine(UIAnimations.instance.ShowQueue(queuePanelRTransform));
    }

    public void EndQueueUI()
    {
        StartCoroutine(UIAnimations.instance.HideQueue(queuePanelRTransform));
    }

    public void HealthChange(int queueIndex, int healthUpdate, bool isPlayer, bool heal)
    {
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            Transform element = panelElements[queueIndex].transform;
            TMP_Text healthText = element.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
            healthText.text = healthUpdate.ToString();

            // Some juice on the UI is needed as well.
            StartCoroutine(UIAnimations.instance.HealthFlash(healthText));
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

    public void DeathChange(int queueIndex)
    {
        GameObject skull = Instantiate(deathPanel, Vector2.zero, Quaternion.identity, panelElements[queueIndex].transform);
        RectTransform skullRect = skull.GetComponent<RectTransform>();
        skullRect.anchoredPosition = Vector2.zero;
        RectTransform elementRect = panelElements[queueIndex].GetComponent<RectTransform>();

        for(int i = queueIndex + 1; i < panelElements.Count; ++i)
        {
            RectTransform entry = panelElements[i].GetComponent<RectTransform>();
            float yValue = entry.anchoredPosition.y + 100f;
            Vector2 newPos = new Vector2(entry.anchoredPosition.x, yValue);
            
            RearrangementElement rearrangementElement = new RearrangementElement(entry.anchoredPosition, newPos, entry);
            UIAnimations.instance.rearrangementElements.Add(rearrangementElement);
        }

        panelElements.RemoveAt(queueIndex);

        StartCoroutine(UIAnimations.instance.HideElement(elementRect));
    }

    private void PopulateQueueUI(List<GameObject> combatQueue)
    {
        float queuePanelHeight = combatQueue.Count * 100f;
        queuePanelRTransform.sizeDelta = new Vector2(queuePanelWidth, queuePanelHeight);

        foreach(GameObject combatUnit in combatQueue)
        {
            Unit unit = combatUnit.GetComponent<Unit>();

            string unitName = unit.acronym;
            int unitHealth = unit.health;
            int unitCombatPoints = unit.combatPoints;
            Sprite unitImage = unit.combatQueueImage;

            GameObject elementInstance = Instantiate(queueElement, Vector2.zero, Quaternion.identity, queuePanel);
            panelElements.Add(elementInstance);

            Image img = elementInstance.transform.Find("HLayout1").Find("UnitImage").GetComponent<Image>();
            TMP_Text nText = elementInstance.transform.Find("HLayout1").Find("UnitName").GetComponent<TMP_Text>();
            TMP_Text hText = elementInstance.transform.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
            TMP_Text cText = elementInstance.transform.Find("HLayout2").Find("HLayout2").Find("UnitCP").GetComponent<TMP_Text>();

            img.sprite = unitImage;
            nText.text = unitName;
            hText.text = unitHealth.ToString();
            cText.text = unitCombatPoints.ToString() + "/" + unitCombatPoints.ToString();

            // Since the position are not driven by a layout group, we have to do it manually.
            RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
            Vector2 newPos = new Vector2(queuePanelRTransform.anchoredPosition.x, queuePanelRTransform.anchoredPosition.y + (combatQueue.Count-panelElements.Count+1)*100f); // what the fuck
            elementRTransform.anchoredPosition = newPos;
        }
    }

    public void UpdateCombatPoints(int currentCombatPoints, int maxCombatPoints, int combatQueueIndex)
    {
        if(updatingCombatPoints == null)
            updatingCombatPoints = GetTextFromElement(combatQueueIndex);
        
        updatingCombatPoints.text = currentCombatPoints + "/" + maxCombatPoints;
    }

    public void RefreshCombatPoints()
    {
        for(int i = 0; i < panelElements.Count; ++i)
        {
            TMP_Text cpText = GetTextFromElement(i);
            int combatPoints = CombatManager.instance.combatQueue[i].GetComponent<Unit>().combatPoints;
            cpText.text = combatPoints.ToString() + "/" + combatPoints.ToString();
        }
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

    private TMP_Text GetTextFromElement(int index)
    {
        return panelElements[index].transform.Find("HLayout2").Find("HLayout2").Find("UnitCP").GetComponent<TMP_Text>();
    }
}
