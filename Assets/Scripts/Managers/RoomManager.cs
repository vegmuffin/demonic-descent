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
    [Space]
    [SerializeField] private float cameraPanSpeed = default;

    private float xDistThreshold;
    private float yDistThreshold;

    private Transform playerTransform;

    private bool isCameraPanning = false;
    private bool isInIntersection = false;
    private float cameraTimer = 0f;
    private Transform cameraTransform;

    private void Awake()
    {
        instance = this;
        playerTransform = GameObject.Find("Player").transform;
        xDistThreshold = roomWidth/2 + 1;
        yDistThreshold = roomHeight/2 + 1;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckCamera();
    }

    private void CheckCamera()
    {
        Vector2 playerPos = playerTransform.position;
        Vector3 roomPos = currentRoom.position;

        float distX = playerPos.x - roomPos.x;
        float distY = playerPos.y - roomPos.y;

        // Left
        if(distX < -xDistThreshold && !isCameraPanning && !isInIntersection)
        {
            float newX = roomPos.x - roomWidth/2 - offsetBetweenRooms;
            isCameraPanning = true;
            Vector3 endPos = new Vector3(newX, roomPos.y, -10f);
            StartCoroutine(CameraPan(roomPos, endPos));
        }
    }

    private IEnumerator CameraPan(Vector3 startPos, Vector3 endPos)
    {
        while(cameraTimer <= 1f)
        {
            cameraTransform.position = Vector2.Lerp(startPos, endPos, cameraTimer);
            cameraTimer += Time.deltaTime * cameraPanSpeed;

            if(cameraTimer >= 1f)
            {
                cameraTimer = 0;
                //cameraTransform.position;
                isCameraPanning = false;
                isInIntersection = true;
                yield break;
            }
            else
            {
                yield return new WaitForSecondsRealtime(Time.deltaTime);
            }
        }
    }

    public void SetRoom(Vector2 roomPos)
    {
        foreach(Room room in allRooms)
        {
            if(room.position == roomPos)
            {
                previousRoom = currentRoom;
                currentRoom = room;
            }
        }
    }
}
