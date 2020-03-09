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
    [Space]
    [SerializeField] private float queuePanelDropSpeed;
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
        float x = queuePanelRTransform.anchoredPosition.x;
        Vector2 startPos = new Vector2(x, queuePanelRTransform.anchoredPosition.y - 50);
        Vector2 endPos = new Vector2(-50, queuePanelRTransform.anchoredPosition.y - 50);

        PopulateQueueUI(combatQueue);

        StartCoroutine(LowerQueueUI(startPos, endPos));
    }

    public void HealthChange(int queueIndex, int healthUpdate)
    {
        Transform element = panelElements[queueIndex].transform;
        TMP_Text healthText = element.Find("HLayout2").Find("HLayout1").Find("UnitHealth").GetComponent<TMP_Text>();
        healthText.text = healthUpdate.ToString();

        // Some juice on the UI is needed as well.
    }

    public void DeathChange(int queueIndex)
    {

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
        }
    }



    private IEnumerator LowerQueueUI(Vector2 startPos, Vector2 endPos)
    {
        while(panelTimer <= 1f)
        {
            panelTimer += Time.deltaTime * queuePanelDropSpeed;
            queuePanelRTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, panelTimer);

            if(panelTimer >= 1f)
            {
                Debug.Log("test");
                panelTimer = 0f;
                queuePanelRTransform.anchoredPosition = endPos;

                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }
}
