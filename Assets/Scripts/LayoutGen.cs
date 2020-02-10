using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGen : MonoBehaviour
{

    [SerializeField] private GameObject square;

    void Start()
    {
        int pathLength = 10;
        bool[,] grid = GenerateLayout(pathLength);
        Visualize(ref grid);
    }

    public bool[,] GenerateLayout(int pathLength)
    {
        bool[,] gridArray = new bool[pathLength+1, pathLength+1];

        // Filling all values with false initially
        for(int x = 0; x < pathLength+1; ++x)
        {
            for(int y = 0; y < pathLength+1; ++y)
            {
                gridArray[x,y] = false;
            }
        }

        // Generating initial path
        int pathX = Random.Range(0, pathLength);
        int pathY = Random.Range(0, pathLength);

        // Saving the starting point
        int startingX = pathX;
        int startingY = pathY;

        int pathLeft = pathLength;
        gridArray[pathX, pathY] = true;
        Debug.Log("START: x - " + pathX + ", y - " + pathY);

        while(pathLeft > 0)
        {
            int randomPath = Random.Range(1, 4);
            bool foundValidPath = false;
            switch(randomPath)
            {
                // Left
                case 1:
                    int minusX = pathX-1;
                    if(minusX < 0)
                        break;
                    if(!gridArray[minusX, pathY])
                    {
                        if(TryCoordinate(minusX, pathY, ref gridArray))
                        {
                            foundValidPath = true;
                            pathX = minusX;
                        }
                    }
                    break;

                // Right
                case 2:
                    int plusX = pathX+1;
                    if(plusX >= gridArray.GetLength(0))
                        break;
                    if(!gridArray[plusX, pathY])
                    {
                        if(TryCoordinate(plusX, pathY, ref gridArray))
                        {
                            pathX = plusX;
                            foundValidPath = true;
                        }
                    }
                    break;

                // Up
                case 3:
                    int plusY = pathY+1;
                    if(plusY >= gridArray.GetLength(0))
                        break;
                    if(!gridArray[pathX, plusY])
                    {
                        if(TryCoordinate(pathX, plusY, ref gridArray))
                        {
                            pathY = plusY;
                            foundValidPath = true;
                        }
                    }
                            
                    break;

                // Down
                case 4:
                    int minusY = pathY-1;
                    if(minusY < 0)
                        break;
                    if(!gridArray[pathX, minusY])
                    {
                        if(TryCoordinate(pathX, minusY, ref gridArray))
                        {
                            pathY = minusY;
                            foundValidPath = true;
                        }
                    }
                    break;
            }
            if(foundValidPath)
            {
                --pathLeft;
                Debug.Log("x - " + pathX + ", y - " + pathY);
                gridArray[pathX, pathY] = true;
            }
        }

        return gridArray;
    }

    private bool TryCoordinate(int x, int y, ref bool[,] gridArray)
    {
        // Checking neighbours
        int neighbourCount = 0;
        if(x-1 < 0)
        {
            // Nothing
            
        } else
        {
            if(gridArray[x-1, y])
            {
                ++neighbourCount;
            }
        }
        if(x+1 >= gridArray.GetLength(0))
        {
            // Nothing
            
        } else
        {
            if(gridArray[x+1, y])
            {
                ++neighbourCount;
            }
        }
        if(y+1 >= gridArray.GetLength(0))
        {
            // Nothing
        } else
        {
            if(gridArray[x, y+1])
            {
                ++neighbourCount;
            }
        }
        if(y-1 < 0)
        {
            // Nothing
        } else
        {
            if(gridArray[x, y-1])
            {
                ++neighbourCount;
            }
        }
        return neighbourCount > 1 ? false : true;
    }

    private void Visualize(ref bool[,] gridArray)
    {
        for(int i = 0; i < gridArray.GetLength(0)-1; ++i)
        {
            for(int j = 0; j < gridArray.GetLength(1)-1; ++j)
            {
                //Debug.Log("x: " + i + ", y: " + j);
                if(gridArray[i,j])
                {
                    Instantiate(square, new Vector2(i, j), Quaternion.identity);
                }
                    
            }
        }
    }
}
