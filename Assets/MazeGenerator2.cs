using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MazeGenerator2 : MonoBehaviour
{
    public int width, height;
    public Material brick;
    private int[,] Maze;
    private List<Vector3> pathMazes = new List<Vector3>();
    private Stack<Vector2> _tiletoTry = new Stack<Vector2>();
    private List<Vector2> offsets = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    private System.Random rnd = new System.Random();
    private int _width, _height;
    private Vector2 _currentTile;
    private Vector2 entrance;
    private Vector2 exit;

    public Vector2 CurrentTile
    {
        get { return _currentTile; }
        private set
        {
            if (value.x < 1 || value.x >= this.width - 1 || value.y < 1 || value.y >= this.height - 1)
            {
                throw new ArgumentException("CurrentTile must be within the one tile border all around the maze");
            }
            if (value.x % 2 == 1 || value.y % 2 == 1)
            { _currentTile = value; }
            else
            {
                throw new ArgumentException("The current square must not be both on an even X-axis and an even Y-axis, to ensure we can get walls around all tunnels");
            }
        }
    }

    private static MazeGenerator2 instance;
    public static MazeGenerator2 Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 30;
        GenerateMaze();
    }

    void GenerateMaze()
    {
        Maze = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Maze[x, y] = 1;
            }
        }

        CurrentTile = Vector2.one;
        _tiletoTry.Push(CurrentTile);
        Maze = CreateMaze();

        // Set Entrance and Exit
        SetEntranceAndExit();

        GameObject ptype = null;

        for (int i = 0; i <= Maze.GetUpperBound(0); i++)
        {
            for (int j = 0; j <= Maze.GetUpperBound(1); j++)
            {
                if (Maze[i, j] == 1)
                {
                    ptype = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    ptype.transform.position = new Vector3(i * ptype.transform.localScale.x, j * ptype.transform.localScale.y, 0);
                    if (brick != null)
                    {
                        ptype.GetComponent<Renderer>().material = brick;
                    }
                    ptype.transform.parent = transform;
                }
                else if (Maze[i, j] == 0)
                {
                    pathMazes.Add(new Vector3(i, j, 0));
                }

            }
        }
    }

    public int[,] CreateMaze()
    {
        List<Vector2> neighbors;

        while (_tiletoTry.Count > 0)
        {
            Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;
            neighbors = GetValidNeighbors(CurrentTile);

            if (neighbors.Count > 0)
            {
                _tiletoTry.Push(CurrentTile);
                CurrentTile = neighbors[rnd.Next(neighbors.Count)];
            }
            else
            {
                CurrentTile = _tiletoTry.Pop();
            }
        }

        return Maze;
    }
    private void SetEntranceAndExit()
    {
        // Randomly choose which corner the entrance will be at
        if (rnd.Next(2) == 0)
        {
            // Entrance at top-left, exit at bottom-right
            entrance = new Vector2(1, 0);  // Top-left corner (just inside the border)
            exit = new Vector2(width - 2, height - 1);  // Bottom-right corner (just inside the border)
        }
        else
        {
            // Entrance at top-right, exit at bottom-left
            entrance = new Vector2(width - 2, 0);  // Top-right corner
            exit = new Vector2(1, height - 1);  // Bottom-left corner
        }

        // Mark the entrance and exit as open paths (0 in the maze)
        Maze[(int)entrance.x, (int)entrance.y] = 0;
        Maze[(int)exit.x, (int)exit.y] = 0;

        // Optionally, you can visualize the entrance and exit with debug logs
        Debug.Log("Entrance: " + entrance);
        Debug.Log("Exit: " + exit);
    }


    private List<Vector2> GetValidNeighbors(Vector2 centerTile)
    {
        List<Vector2> validNeighbors = new List<Vector2>();

        foreach (var offset in offsets)
        {
            Vector2 toCheck = new Vector2(centerTile.x + offset.x, centerTile.y + offset.y);

            if (toCheck.x % 2 == 1 || toCheck.y % 2 == 1)
            {
                if (Maze[(int)toCheck.x, (int)toCheck.y] == 1 && HasThreeWallsIntact(toCheck))
                {
                    validNeighbors.Add(toCheck);
                }
            }
        }

        return validNeighbors;
    }

    private bool HasThreeWallsIntact(Vector2 Vector2ToCheck)
    {
        int intactWallCounter = 0;

        foreach (var offset in offsets)
        {
            Vector2 neighborToCheck = new Vector2(Vector2ToCheck.x + offset.x, Vector2ToCheck.y + offset.y);

            if (IsInside(neighborToCheck) && Maze[(int)neighborToCheck.x, (int)neighborToCheck.y] == 1)
            {
                intactWallCounter++;
            }
        }

        return intactWallCounter == 3;
    }

    private bool IsInside(Vector2 p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }
}
