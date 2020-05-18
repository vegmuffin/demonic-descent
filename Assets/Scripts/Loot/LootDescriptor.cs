using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootDescriptor : MonoBehaviour
{
    private GameObject item;

    public void SetValues(string itemDescription, GameObject item)
    {
        var tmp = transform.GetChild(0).GetComponent<TMP_Text>();
        var tmpRect = transform.GetComponent<RectTransform>();

        tmp.text = itemDescription;

        float sizeY = tmp.preferredHeight;
        float sizeX = tmp.preferredWidth;
        tmp.fontSize -= 0.5f;

        tmpRect.sizeDelta = new Vector2(sizeX, sizeY);

        this.item = item;
    }

    public void OnClick()
    {
        item.GetComponent<LootItem>().PickUp();
    }
}
