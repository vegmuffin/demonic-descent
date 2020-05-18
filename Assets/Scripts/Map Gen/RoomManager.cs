using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [HideInInspector] public Room currentRoom;
    [HideInInspector] public Room previousRoom;
    private Room roomToBe;
    [HideInInspector] public List<Room> allRooms = new List<Room>();

    public int roomWidth;
    public int roomHeight;
    public int offsetBetweenRooms;
    [Header("Only odd numbers for intersection width to retain symmetry")]
    public int intersectionWidth;
    [Space]
    [SerializeField] private AnimationCurve cameraPanSpeedCurve = default;

    private float xDistThreshold;
    private float yDistThreshold;

    private Transform playerTransform;

    private bool isCameraPanning = false;
    private bool isInIntersection = false;
    private Transform cameraTransform;
    private Vector2 dir = Vector2.zero;

    private void Awake()
    {
        instance = this;
        playerTransform = GameObject.Find("Player").transform;
        xDistThreshold = roomWidth;
        yDistThreshold = roomHeight;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckCamera();
    }

    private void CheckCamera()
    {
        Vector2 playerPos = playerTransform.position;
        Vector2 roomPos = currentRoom.position;

        float distX = playerPos.x - roomPos.x;
        float distY = playerPos.y - roomPos.y;


        if(!isCameraPanning)
        {
            // If we are not in an intersection, check some threshold values if we have to move the camera.
            if(!isInIntersection)
            {
                // Left
                if(distX < -xDistThreshold)
                {
                    float newX = roomPos.x-(roomWidth+offsetBetweenRooms);
                    isCameraPanning = true;
                    Vector3 endPos = new Vector3(newX, roomPos.y+1, -10f);
                    Vector3 startPos = new Vector3(currentRoom.position.x, currentRoom.position.y+1, -10f);
                    dir = Vector2.left;

                    roomToBe = GetRoom(new Vector2(roomPos.x-(roomWidth*2+offsetBetweenRooms*2), roomPos.y));

                    StartCoroutine(CameraPan(startPos, endPos));
                }

                // Right
                if(distX > xDistThreshold)
                {
                    float newX = roomPos.x + roomWidth + offsetBetweenRooms;
                    isCameraPanning = true;
                    Vector3 endPos = new Vector3(newX, roomPos.y+1, -10f);
                    Vector3 startPos = new Vector3(currentRoom.position.x, currentRoom.position.y+1, -10f);
                    dir = Vector2.right;

                    roomToBe = GetRoom(new Vector2(roomPos.x+(roomWidth*2+offsetBetweenRooms*2), roomPos.y));

                    StartCoroutine(CameraPan(startPos, endPos));
                }

                // Bottom
                if(distY < -yDistThreshold)
                {
                    float newY = roomPos.y - (roomHeight + offsetBetweenRooms);
                    isCameraPanning = true;
                    Vector3 endPos = new Vector3(roomPos.x, newY+1, -10f);
                    Vector3 startPos = new Vector3(currentRoom.position.x, currentRoom.position.y+1, -10f);
                    dir = Vector2.down;

                    roomToBe = GetRoom(new Vector2(roomPos.x, roomPos.y-(roomHeight*2+offsetBetweenRooms*2)));

                    StartCoroutine(CameraPan(startPos, endPos));
                }

                // Top
                if(distY > yDistThreshold)
                {
                    float newY = roomPos.y + roomHeight + offsetBetweenRooms;
                    isCameraPanning = true;
                    Vector3 endPos = new Vector3(roomPos.x, newY+1, -10f);
                    Vector3 startPos = new Vector3(currentRoom.position.x, currentRoom.position.y+1, -10f);
                    dir = Vector2.up;

                    roomToBe = GetRoom(new Vector2(roomPos.x, roomPos.y+(roomHeight*2+offsetBetweenRooms*2)));

                    StartCoroutine(CameraPan(startPos, endPos));
                }
            } 
            else
            {
                // Intersection part, two checks need to be made, for the next room after the intersection and for the one we have come from.
                if(dir == Vector2.left)
                {
                    if(playerPos.x < roomToBe.position.x+roomWidth)
                    {
                        Vector3 endPos = PrepareTransition("next");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                    else if(playerPos.x > currentRoom.position.x-roomWidth)
                    {
                        Vector3 endPos = PrepareTransition("current");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                }
                if(dir == Vector2.right)
                {
                    if(playerPos.x > roomToBe.position.x-roomWidth)
                    {
                        Vector3 endPos = PrepareTransition("next");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                    else if(playerPos.x < currentRoom.position.x+roomWidth)
                    {
                        Vector3 endPos = PrepareTransition("current");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                }
                if(dir == Vector2.down)
                {
                    if(playerPos.y < roomToBe.position.y+roomHeight)
                    {
                        Vector3 endPos = PrepareTransition("next");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                    else if(playerPos.y > currentRoom.position.y-roomHeight)
                    {
                        Vector3 endPos = PrepareTransition("current");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                }
                if(dir == Vector2.up)
                {
                    if(playerPos.y > roomToBe.position.y-roomHeight)
                    {
                        Vector3 endPos = PrepareTransition("next");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                    else if(playerPos.y < currentRoom.position.y+roomHeight)
                    {
                        Vector3 endPos = PrepareTransition("current");
                        StartCoroutine(CameraPan(cameraTransform.position, endPos));
                    }
                }
            }
            
        }
        
    }

    private Vector3 PrepareTransition(string whichRoom)
    {
        isCameraPanning = true;
        dir = Vector2.zero;
        // If we are back at the current room, nothing has to be changed.
        if(whichRoom == "current")
        {
            return new Vector3(currentRoom.position.x, currentRoom.position.y+1, -10f);
        }
        // Updating the current room and the previous room.
        else if(whichRoom == "next")
        {
            previousRoom = currentRoom;
            currentRoom = roomToBe;

            CombatManager.instance.enemyList.Clear();
            foreach(GameObject enemy in currentRoom.enemyList)
                CombatManager.instance.enemyList.Add(enemy);

            return new Vector3(roomToBe.position.x, roomToBe.position.y+1, -10f);
        }
        return Vector3.zero;
    }

    private IEnumerator CameraPan(Vector3 startPos, Vector3 endPos)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            cameraTransform.position = Vector3.Lerp(startPos, endPos, timer);
            timer += Time.deltaTime * cameraPanSpeedCurve.Evaluate(timer);

            if(timer >= 1f)
            {
                cameraTransform.position = endPos;
                isCameraPanning = false;
                isInIntersection = !isInIntersection;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }

        yield break;
    }

    private Room GetRoom(Vector2 roomPos)
    {
        foreach(Room room in allRooms)
            if(room.position == roomPos)
                return room;
        return null;
    }

}
