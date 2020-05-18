using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    [HideInInspector] public int goldCollected = 0;
    [HideInInspector] public Transform worldSpaceCanvas;
    [HideInInspector] public bool isSpacePressed = false;
    public GameObject lootDescription = default;

    [HideInInspector] public Dictionary<GameObject, GameObject> lootDict = new Dictionary<GameObject, GameObject>();

    public float yOffset = default;

    private void Awake()
    {
        instance = this;
        worldSpaceCanvas = GameObject.Find("WorldSpaceCanvas").transform;
    }

    private void Update()
    {
        IsSpacePressed();
    }

    private void IsSpacePressed()
    {
        if(GameStateManager.instance.CheckState("SNOOZE"))
            return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isSpacePressed = true;
            ShowItems();
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            isSpacePressed = false;
            HideItems();
        }
    }

    private void ShowItems()
    {
        foreach(KeyValuePair<GameObject, GameObject> dictItem in lootDict)
        {
            dictItem.Value.SetActive(true);
        }
    }

    private void HideItems()
    {
        foreach(KeyValuePair<GameObject, GameObject> dictItem in lootDict)
        {
            dictItem.Value.SetActive(false);
        }
    }

    public void ShowSingleItem(GameObject key)
    {
        CursorManager.instance.inUse = true;

        GameObject descriptor = lootDict[key];
        if(descriptor.activeSelf)
            return;

        descriptor.SetActive(true);
    }

    public void HideSingleItem(GameObject key)
    {
        CursorManager.instance.inUse = false;

        GameObject descriptor = lootDict[key];
        if(isSpacePressed)
            return;

        descriptor.SetActive(false);
    }

    public void DepleteGold(int amount)
    {
        goldCollected -= amount;
        UIManager.instance.UpdateGoldText(goldCollected);
    }
}
