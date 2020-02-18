using UnityEngine;

public class Room
{
    public bool isExplored;
    public Vector2 position;

    public Room(bool isExplored, Vector2 position)
    {
        this.isExplored = isExplored;
        this.position = position;
    }
}