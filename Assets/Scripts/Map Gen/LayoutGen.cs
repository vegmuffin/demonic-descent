using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGen : MonoBehaviour
{
    [SerializeField] private int forkingChance;
    [SerializeField] private float forkingDiminishing;
    
    private List<Vector2> roomCoordinates = new List<Vector2>();

    private bool exitCondition = false;
    private bool alarm = false;
    private bool forking = false;
    private int neighbours;
    [Space]
    [Header("For testing")]
    [SerializeField] private int primaryLength;

    private void Start()
    {
        // First it generates the primary layout (a single path)
        GenerateLayout(primaryLength);
        // Then it tries to generate random branches from the above made path
        GenerateForks();
        // Based on the layout that we generated, it generates all the tiles
        TilemapGen.instance.Generate(roomCoordinates);
    }

    // Generates the primary path for the layout.
    private void GenerateLayout(int primaryPathLength)
    {
        Vector2 currentCoord = Vector2.zero;
        roomCoordinates.Add(currentCoord);

        int pathLeft = primaryPathLength;
        while(pathLeft > 0 && !exitCondition)
        {
            Vector2 newCoord = GiveCoordinate(currentCoord);
            if(newCoord == currentCoord)
            {
                
                continue;
            }
            currentCoord = newCoord;
            roomCoordinates.Add(currentCoord);
            --pathLeft;
        }
        if(exitCondition)
            exitCondition = false;
    }

    // Generates forks from the primary path to give a more 'mappy' feel, rather than a single path to take.
    private void GenerateForks()
    {
        forking = true;
        int length = roomCoordinates.Count;
        for(int i = 0; i < length; ++i)
        {
            float chanceToFork = forkingChance;
            ForkRecursion(roomCoordinates[i], chanceToFork);
            if(exitCondition)
                exitCondition = false;
        }
    }

    // Each fork has a chance to create additional rooms from its own path.
    private void ForkRecursion(Vector2 coord, float chance)
    {
        int random = Random.Range(0, 100);
        if(chance > random)
        {
            /* Debug.Log("Fork success!"); */
            bool foundPath = false;
            while(!foundPath && !exitCondition)
            {
                Vector2 newCoord = GiveCoordinate(coord);
                if(newCoord == coord)
                {
                    continue;
                }

                coord = newCoord;
                roomCoordinates.Add(coord);
                foundPath = true;
                ForkRecursion(coord, chance/forkingDiminishing);
            }
        }
    }

    // Give a random coordinate (left/right/bottom/top) coordinate to expand to.
    private Vector2 GiveCoordinate(Vector2 currentCoord)
    {
        int randomCoord = Random.Range(1, 5);
        Vector2 coordToGive = currentCoord;
        switch(randomCoord)
        {
            // Left
            case 1:
                Vector2 leftCoord = new Vector2(currentCoord.x-1, currentCoord.y);
                if(!ContainsCoord(leftCoord))
                    if(TryCoordinate(leftCoord, currentCoord))
                        coordToGive = leftCoord;
                break;
            // Right
            case 2:
                Vector2 rightCoord = new Vector2(currentCoord.x+1, currentCoord.y);
                if(!ContainsCoord(rightCoord))
                    if(TryCoordinate(rightCoord, currentCoord))
                        coordToGive = rightCoord;
                break;
            // Bottom
            case 3:
                Vector2 bottomCoord = new Vector2(currentCoord.x, currentCoord.y-1);
                if(!ContainsCoord(bottomCoord))
                    if(TryCoordinate(bottomCoord, currentCoord))
                        coordToGive = bottomCoord;
                break;
            // Top
            case 4:
                Vector2 topCoord = new Vector2(currentCoord.x, currentCoord.y+1);
                if(!ContainsCoord(topCoord))
                    if(TryCoordinate(topCoord, currentCoord))
                        coordToGive = topCoord;
                break;
        }
        return coordToGive;
    }

    // Can we expand in the given coordinate?
    private bool TryCoordinate(Vector2 coord, Vector2 currentCoord)
    {
        // The future room should only have one neighbour, the one we branched out from. Checking all directions.

        if(!alarm)
            neighbours = 0;
        
        Vector2 checkCoord = Vector2.zero;

        // Check left
        checkCoord = new Vector2(coord.x-1, coord.y);
        CheckDirection(checkCoord);
        
        // Check right
        checkCoord = new Vector2(coord.x+1, coord.y);
        CheckDirection(checkCoord);

        // Check bottom
        checkCoord = new Vector2(coord.x, coord.y-1);
        CheckDirection(checkCoord);

        // Check top
        checkCoord = new Vector2(coord.x, coord.y+1);
        CheckDirection(checkCoord);

        // If there are more neighbours, we have to check if we reached a dead-end, otherwise it will try random coordinates with no avail.
        if((neighbours >= 2) && !alarm)
        {
            alarm = true;
            if(CheckDeadend(currentCoord))
            {
                Debug.Log("EXIT CONDITION DETECTED");
                exitCondition = true;
                alarm = false;
            }
        }

        return (neighbours == 1 || neighbours == 0) ? true : false;
    }

    // Check if a room exists at a given coordinate. If it does, increase the neighbour count.
    private void CheckDirection(Vector2 checkCoord)
    {
        foreach(Vector2 existingCoord in roomCoordinates)
            if(checkCoord == existingCoord)
                neighbours++;
    }

    // A lot of the time the path may be stuck somewhere with no room to expand, this functionality ends the path preemptively if that happens.
    private bool CheckDeadend(Vector2 coord)
    {
        neighbours = 0;
        int tempNeighbours = 0;

        // Sometimes the amount of neighbours is vast but there is still room to expand, this bool tells us whether there is room to expand to.
        bool isThereRoomLeft = false;

        // Left
        Vector2 checkCoord = new Vector2(coord.x-1, coord.y);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord, Vector2.zero);
        if(neighbours == 1)
            isThereRoomLeft = true;
        tempNeighbours = neighbours;

        // Right
        checkCoord = new Vector2(coord.x+1, coord.y);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord, Vector2.zero);
        if(neighbours - tempNeighbours == 1)
            isThereRoomLeft = true;
        tempNeighbours = neighbours;


        // Bottom
        checkCoord = new Vector2(coord.x, coord.y-1);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord, Vector2.zero);
        if(neighbours - tempNeighbours == 1)
            isThereRoomLeft = true;
        tempNeighbours = neighbours;

        // Top
        checkCoord = new Vector2(coord.x, coord.y+1);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord, Vector2.zero);
        if(neighbours - tempNeighbours == 1)
            isThereRoomLeft = true;

        alarm = false;
        if(((neighbours >= 6 && !forking) || (neighbours >= 4 && forking)) && !isThereRoomLeft)
            return true;
        else
            return false;
    }

    // Check if a room exists in a given coordinate.
    private bool ContainsCoord(Vector2 coord)
    {
        foreach(Vector2 roomCoord in roomCoordinates)
            if(coord == roomCoord)
                return true;
        return false;
    }

}
