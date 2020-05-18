using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int cost;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShopManager.instance.OnItemHover(this, cost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShopManager.instance.OnItemExitHover();
    }
}
