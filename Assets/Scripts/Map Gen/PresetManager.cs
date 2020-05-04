using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    public static PresetManager instance;

    [Header("Presets with one intersection")]
    public List<GameObject> I1Bottom = new List<GameObject>();
    public List<GameObject> I1Left = new List<GameObject>();
    public List<GameObject> I1Top = new List<GameObject>();
    public List<GameObject> I1Right = new List<GameObject>();

    [Header("Presets with two intersections")]
    public List<GameObject> I2LeftAndTop = new List<GameObject>();
    public List<GameObject> I2TopAndRight = new List<GameObject>();
    public List<GameObject> I2RightAndBottom = new List<GameObject>();
    public List<GameObject> I2BottomAndLeft = new List<GameObject>();
    public List<GameObject> I2TopAndBottom = new List<GameObject>();
    public List<GameObject> I2LeftAndRight = new List<GameObject>();

    [Header("Presets with three intersections")]
    public List<GameObject> I3LeftTopAndRight = new List<GameObject>();
    public List<GameObject> I3TopRightAndBottom = new List<GameObject>();
    public List<GameObject> I3RightBottomAndLeft = new List<GameObject>();
    public List<GameObject> I3BottomLeftAndTop = new List<GameObject>();

    [Header("Presets with four intersections")]
    public List<GameObject> I4 = new List<GameObject>();
    
    private void Awake()
    {
        instance = this;
    }

    public GameObject InstantiatePreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection, Vector2 pos)
    {
        if(lIntersection && !tIntersection && !rIntersection && !bIntersection)
        {
            GameObject go = GetPreset(lIntersection, tIntersection, rIntersection, bIntersection);
            GameObject newInstance = Instantiate(go, pos, Quaternion.identity);

            return newInstance;
        }

        return null;
        
    }

    private GameObject GetPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(GetBottomPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I1Bottom);
        if(GetLeftPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I1Left);
        if(GetTopPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I1Top);
        if(GetRightPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I1Right);
        
        if(GetLeftTopPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2LeftAndTop);
        if(GetTopRightPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2TopAndRight);
        if(GetRightBottomPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2RightAndBottom);
        if(GetBottomLeftPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2BottomAndLeft);
        if(GetTopBottomPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2TopAndBottom);
        if(GetRightLeftPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I2LeftAndRight);

        if(GetLeftTopRightPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I3LeftTopAndRight);
        if(GetTopRightBottomPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I3TopRightAndBottom);
        if(GetRightBottomLeftPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I3RightBottomAndLeft);
        if(GetBottomLeftTopPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I3BottomLeftAndTop);

        if(GetAllPreset(lIntersection, tIntersection, rIntersection, bIntersection))
            return GetPresetFromList(I4);
        
        // Something wrong occured if we got here.
        return null;
    }

    private GameObject GetPresetFromList(List<GameObject> presetList)
    {
        int ran = Random.Range(0, presetList.Count);
        return presetList[ran];
    }

    private bool GetLeftPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(lIntersection && !tIntersection && !rIntersection && !bIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetTopPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(tIntersection && !rIntersection && !bIntersection && !lIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetBottomPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(bIntersection && !rIntersection && !lIntersection && !tIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetRightPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(rIntersection && !lIntersection && !tIntersection && !bIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetLeftTopPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(lIntersection && tIntersection && !bIntersection && !rIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetTopRightPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(tIntersection && rIntersection && !bIntersection && !lIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetRightBottomPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(rIntersection && bIntersection && !lIntersection && !tIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetBottomLeftPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(bIntersection && lIntersection && !rIntersection && !tIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetTopBottomPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(tIntersection && bIntersection && !lIntersection && !rIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetRightLeftPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(rIntersection && lIntersection && !tIntersection && !bIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetLeftTopRightPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(lIntersection && tIntersection && rIntersection && !bIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetTopRightBottomPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(tIntersection && rIntersection && bIntersection && !lIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetRightBottomLeftPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(rIntersection && bIntersection && lIntersection && !tIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetBottomLeftTopPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(bIntersection && lIntersection && tIntersection && !rIntersection)
        {
            return true;
        }
        return false;
    }

    private bool GetAllPreset(bool lIntersection, bool tIntersection, bool rIntersection, bool bIntersection)
    {
        if(lIntersection && tIntersection && rIntersection && bIntersection)
        {
            return true;
        }
        return false;
    }

}
