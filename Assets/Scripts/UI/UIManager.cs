using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private GameObject queueElement;
    [SerializeField] private GameObject deathPanel;
    [Space]
    [SerializeField] private float queuePanelWidth;

    private Transform canvasTransform;
    private Transform queuePanel;
    private RectTransform queuePanelRTransform;
    private List<GameObject> panelElements = new List<GameObject>();
    private float panelTimer = 0f;

    private void Awake()
    {
        instance = this;
        canvasTransform = GameObject.Find("Canvas").transform;
        queuePanel = canvasTransform.GetChild(0);
        queuePanelRTransform = queuePanel.GetComponent<RectTransform>();
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

    public void HealthChange(int queueIndex, int healthUpdate)
    {
        Transform element = panelElements[queueIndex].transform;
        TMP_Text healthText = element.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
        healthText.text = healthUpdate.ToString();

        // Some juice on the UI is needed as well.
        if(GameStateManager.instance.gameState == GameStateManager.GameStates.COMBAT)
            StartCoroutine(UIAnimations.instance.HealthFlash(healthText));
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

    public void ClearQueue()
    {

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
            cText.text = unitCombatPoints.ToString();

            // Since the position are not driven by a layout group, we have to do it manually.
            RectTransform elementRTransform = elementInstance.GetComponent<RectTransform>();
            Vector2 newPos = new Vector2(queuePanelRTransform.anchoredPosition.x, queuePanelRTransform.anchoredPosition.y + (combatQueue.Count-panelElements.Count+1)*100f); // what the fuck
            elementRTransform.anchoredPosition = newPos;
        }
    }
}
