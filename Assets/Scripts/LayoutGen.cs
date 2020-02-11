using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGen : MonoBehaviour
{
    [SerializeField] private GameObject square;
    private List<Vector2> roomCoordinates = new List<Vector2>();
    private bool exitCondition = false;
    private bool alarm = false;
    private int neighbours;

    private void Start()
    {
        GenerateLayout(300);
        Visualize();
    }

    private void GenerateLayout(int primaryPathLength)
    {
        Vector2 currentCoord = Vector2.zero;
        roomCoordinates.Add(currentCoord);

        int pathLeft = primaryPathLength;
        while(pathLeft > 0 && !exitCondition)
        {
            Vector2 newCoord = GiveCoordinate(currentCoord);
            if(newCoord == currentCoord)
                continue;
            currentCoord = newCoord;
            roomCoordinates.Add(currentCoord);
            --pathLeft;
        }
    }

    private void GenerateForks()
    {

    }

    private void ForkRecursion()
    {

    }

    private Vector2 GiveCoordinate(Vector2 currentCoord)
    {
        int randomCoord = Random.Range(1, 5);
        Vector2 coordToGive = currentCoord;
        switch(randomCoord)
        {
            case 1:
                Vector2 leftCoord = new Vector2(currentCoord.x-1, currentCoord.y);
                if(!ContainsCoord(leftCoord))
                    if(TryCoordinate(leftCoord))
                        coordToGive = leftCoord;
                break;
            case 2:
                Vector2 rightCoord = new Vector2(currentCoord.x+1, currentCoord.y);
                if(!ContainsCoord(rightCoord))
                    if(TryCoordinate(rightCoord))
                        coordToGive = rightCoord;
                break;
            case 3:
                Vector2 bottomCoord = new Vector2(currentCoord.x, currentCoord.y-1);
                if(!ContainsCoord(bottomCoord))
                    if(TryCoordinate(bottomCoord))
                        coordToGive = bottomCoord;
                break;
            case 4:
                Vector2 topCoord = new Vector2(currentCoord.x, currentCoord.y+1);
                if(!ContainsCoord(topCoord))
                    if(TryCoordinate(topCoord))
                        coordToGive = topCoord;
                break;
        }
        return coordToGive;
    }

    private bool TryCoordinate(Vector2 coord)
    {
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

        if(neighbours == 3 && !alarm)
        {
            alarm = true;
            if(CheckDeadend(coord))
            {
                Debug.Log("EXIT CONDITION DETECTED");
                exitCondition = true;
            }
        }

        return neighbours > 1 ? false : true;
    }

    private void CheckDirection(Vector2 checkCoord)
    {
        foreach(Vector2 existingCoord in roomCoordinates)
            if(checkCoord == existingCoord)
                ++neighbours;
    }

    private bool CheckDeadend(Vector2 coord)
    {
        neighbours = 0;

        // Left
        Vector2 checkCoord = new Vector2(coord.x-1, coord.y);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord);
        Debug.Log("DEADEND CHECK (LEFT): " + neighbours + "ON COORDINATE x: " + checkCoord.x + ", y: " + checkCoord.y);

        // Right
        checkCoord = new Vector2(coord.x+1, coord.y);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord);
        Debug.Log("DEADEND CHECK (RIGHT): " + neighbours + "ON COORDINATE x: " + checkCoord.x + ", y: " + checkCoord.y);

        // Bottom
        checkCoord = new Vector2(coord.x, coord.y-1);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord);
        Debug.Log("DEADEND CHECK (BOTTOM): " + neighbours + "ON COORDINATE x: " + checkCoord.x + ", y: " + checkCoord.y);

        // Top
        checkCoord = new Vector2(coord.x, coord.y+1);
        if(!ContainsCoord(checkCoord))
            TryCoordinate(checkCoord);
        Debug.Log("DEADEND CHECK (TOP): " + neighbours + "ON COORDINATE x: " + checkCoord.x + ", y: " + checkCoord.y);

        alarm = false;
        if(neighbours == 7)
            return true;
        else
            return false;
    }

    private bool ContainsCoord(Vector2 coord)
    {
        foreach(Vector2 roomCoord in roomCoordinates)
            if(coord == roomCoord)
                return true;
        return false;
    }

    private void Visualize()
    {
        foreach(Vector2 coord in roomCoordinates)
        {
            Instantiate(square, coord, Quaternion.identity);
        }
    }
}
