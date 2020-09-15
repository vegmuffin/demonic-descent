using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [HideInInspector] public Room currentRoom;
    [HideInInspector] public Room previousRoom;
    [HideInInspector] public List<Room> allRooms = new List<Room>();

    public int roomWidth;
    public int roomHeight;
    public int offsetBetweenRooms;
    [Header("Only odd numbers for intersection width to retain symmetry")]
    public int intersectionWidth;

    private void Awake()
    {
        instance = this;
    }

    public void SetRoom(Room r)
    {
        previousRoom = currentRoom;
        currentRoom = r;
        CombatManager.instance.enemyList.Clear();
        foreach (GameObject enemy in currentRoom.enemyList)
            CombatManager.instance.enemyList.Add(enemy);
    }

}
