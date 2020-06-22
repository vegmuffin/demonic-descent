using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    private ShopItem currentItem;

    private GameObject player;
    private Unit playerUnit;
    private Vector2 shopPosition;
    private bool isInRange = false;
    private float timer = 0f;
    private float checkInterval = 0.25f;

    [SerializeField] private GameObject shop = default;
    [SerializeField] private TMP_Text shopInfo = default;
    [SerializeField] [TextArea(5, 5)] private string shopInfoText = default;
    [SerializeField] private float shopInfoWriteInterval = default;
    [SerializeField] private TMP_Text shopItemDescription = default;

    private void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        playerUnit = player.GetComponent<Unit>();
        shopPosition = transform.position;
    }

    private void Update()
    {
        CheckTimer();
    }

    private void CheckTimer()
    {
        timer += Time.deltaTime;

        if(timer >= checkInterval)
        {
            bool previousCheck = isInRange;

            timer = 0f;
            isInRange = CheckDistance();

            if(!previousCheck && isInRange)
                Shop();
            else if(previousCheck && !isInRange)
                EndShop();
        }
    }

    private bool CheckDistance()
    {
        Vector2 playerPos = player.transform.position;

        return (playerPos - shopPosition).sqrMagnitude < 1.5f*1.5f ? true : false;
    }

    private void Shop()
    {
        shop.SetActive(true);
        StartCoroutine(ConstructShopText());
    }

    private void EndShop()
    {
        shop.SetActive(false);
        shopInfo.text = string.Empty;
    }

    public void ShopOnClick()
    {
        // Theoretically, currentItem should never be null, but it doesn't hurt to check.
        if(!currentItem)
        {
            Debug.LogError("Something went wrong while shopping.");
            return;
        }

        if(currentItem.cost > LootManager.instance.goldCollected)
        {
            shopItemDescription.text = "<color=#CC0000>Not enough gold!</color>";
            return;
        }

        // All goes well if we are still in this method.
        shopItemDescription.text = "<color=#33FF33>Item bought!</color>";

        switch(currentItem.gameObject.name)
        {
            case "HealthAddition":
                playerUnit.maxHealth += 1;
                playerUnit.health += 1;
                UIManager.instance.IncreasePlayerHealth(1);
                break;
            case "DamageAddition":
                playerUnit.damage += 1;
                UIManager.instance.IncreaseDamageText(playerUnit.damage);
                break;
            case "CombatPointsAddition":
                playerUnit.combatPoints += 1;
                UIManager.instance.IncreaseCombatPointsText(playerUnit.combatPoints);
                break;
            default:
                break;
        }

        LootManager.instance.DepleteGold(currentItem.cost);
    }

    public void OnItemHover(ShopItem shItem, int cost)
    {
        string itemName = shItem.gameObject.name;
        currentItem = shItem;

        if(itemName == "HealthAddition")
        {
            shopItemDescription.text = "Increases max health by 1.";
        }
        else if(itemName == "DamageAddition")
        {
            shopItemDescription.text = "Increases damage by 1.";
        }
        else if(itemName == "CombatPointsAddition")
        {
            shopItemDescription.text = "Increases max combat points by 1.";
        }
        shopItemDescription.text += "\nCost: <color=#FFFF66>" + cost.ToString() + "</color>";
    }

    public void OnItemExitHover()
    {
        shopItemDescription.text = string.Empty;
        currentItem = null;
    }

    private IEnumerator ConstructShopText()
    {
        StringBuilder sb = new StringBuilder("", 30);
        foreach(char ch in shopInfoText)
        {
            sb.Append(ch);
            shopInfo.text = sb.ToString();

            if(ch == ' ')
                continue;
            else
                yield return new WaitForSecondsRealtime(shopInfoWriteInterval);
        }
        yield break;
    }

    public void InvalidateShopTile()
    {
        Vector3Int shopPos = new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0);
        MovementManager.instance.UpdateTileWalkability(shopPos, false);
    }

}
