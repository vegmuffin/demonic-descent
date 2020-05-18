using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public bool inUse = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.instance.inUse = true;
        inUse = true;
        CursorManager.instance.SetCursor("DEFAULT");
        MovementManager.instance.ResetTilePos();
        
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            CursorManager.instance.DisableHoveringPoints();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.instance.inUse = false;
        inUse = false;
        if(GameStateManager.instance.CheckState("COMBAT"))
        {
            CursorManager.instance.EnableHoveringPoints();
        }
    }

    public void DestroyCheck()
    {
        if(inUse)
        {
            CursorManager.instance.inUse = false;
            inUse = false;
            if(GameStateManager.instance.CheckState("COMBAT"))
            {
                CursorManager.instance.EnableHoveringPoints();
            }
        }
    }
}
