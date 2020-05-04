using UnityEngine;

public class Room
{
    public bool isExplored;
    public Vector2 position;

    private bool bIntersection = false;
    private bool lIntersection = false;
    private bool tIntersection = false;
    private bool rIntersection = false;

    public GameObject preset;

    public Room(bool isExplored, Vector2 position, bool bi, bool li, bool ti, bool ri)
    {
        this.isExplored = isExplored;
        this.position = position;

        bIntersection = bi;
        lIntersection = li;
        tIntersection = ti;
        rIntersection = ri;

        preset = PresetManager.instance.InstantiatePreset(li, ti, ri, bi, position);       
    }
    
}