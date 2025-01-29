using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGenerator : MonoBehaviour
{
    public int width, height;
    public TileBase wallTile; // Reference to the wall tile
    public TileBase floorTile; // Reference to the floor tile
    public Tilemap tilemap; // Reference to the Tilemap

    private int[,] Maze;
    private Stack<Vector2> _tiletoTry = new Stack<Vector2>(); // Stack for tiles to try
    private List<Vector2> offsets = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    private System.Random rnd = new System.Random();
    private Vector2 entrance;
    private Vector2 exit;

    // Declare CurrentTile here as a class member
    private Vector2 CurrentTile;

    void Start()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 30;
        tilemap.ClearAllTiles();  // Clear any existing tiles in the Tilemap
        GenerateMaze();
    }
    void GenerateMaze()
    {
        Maze = new int[width, height];

        // Initialize the maze with walls (1)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Maze[x, y] = 1;
            }
        }

        // Start the maze generation
        Vector2 startTile = new Vector2(1, 0); // Starting point (just inside the boundary)
        _tiletoTry.Push(startTile);
        Maze = CreateMaze();

        // Set Entrance and Exit
        SetEntranceAndExit();

        // Ensure only two floor tiles (entry and exit) are at the boundary
        EnsureCorrectBoundary();

        // Set tiles on the tilemap based on the maze data
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (Maze[x, y] == 1) // Wall tile
                {
                    tilemap.SetTile(tilePosition, wallTile);
                }
                else if (Maze[x, y] == 0) // Floor tile
                {
                    tilemap.SetTile(tilePosition, floorTile);
                }
            }
        }
    }

    private void EnsureCorrectBoundary()
    {
        int floorTileCount = 0;

        // Check for floor tiles on the boundary (edges of the maze)
        for (int x = 0; x < width; x++)
        {
            if (Maze[x, 0] == 0) floorTileCount++; // Top boundary
            if (Maze[x, height - 1] == 0) floorTileCount++; // Bottom boundary
        }
        for (int y = 0; y < height; y++)
        {
            if (Maze[0, y] == 0) floorTileCount++; // Left boundary
            if (Maze[width - 1, y] == 0) floorTileCount++; // Right boundary
        }

        // If there are more than 2 floor tiles at the boundary, replace the starting tile with a wall
        if (floorTileCount > 2)
        {
            Maze[1, 0] = 1; // Replace start tile with wall
        }
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

    public int[,] CreateMaze()
    {
        List<Vector2> neighbors;

        // Start from the initial tile (1,0) or another starting point
        CurrentTile = new Vector2(1, 0); // Initialize CurrentTile
        _tiletoTry.Push(CurrentTile);  // Push starting point to stack

        while (_tiletoTry.Count > 0)
        {
            // Set the current tile as a path (0)
            Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;

            // Get valid neighbors for the current tile
            neighbors = GetValidNeighbors(CurrentTile);

            if (neighbors.Count > 0)
            {
                // Push the current tile to the stack to backtrack later if necessary
                _tiletoTry.Push(CurrentTile);

                // Randomly select a neighbor and set it as the next current tile
                CurrentTile = neighbors[rnd.Next(neighbors.Count)];
                _tiletoTry.Push(CurrentTile);  // Push the next tile to the stack
                Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0; // Mark the next tile as a path (0)
            }
            else
            {
                // Backtrack if no valid neighbors are found
                CurrentTile = _tiletoTry.Pop();
            }
        }

        return Maze;
    }

    private List<Vector2> GetValidNeighbors(Vector2 centerTile)
    {
        List<Vector2> validNeighbors = new List<Vector2>();

        foreach (var offset in offsets)
        {
            Vector2 toCheck = new Vector2(centerTile.x + offset.x, centerTile.y + offset.y);

            // Ensure that the toCheck position is within bounds and is not a wall (1)
            if (IsInside(toCheck))
            {
                // Only check odd-index tiles
                if ((toCheck.x % 2 == 1 || toCheck.y % 2 == 1) && Maze[(int)toCheck.x, (int)toCheck.y] == 1)
                {
                    // Check that the current position has three intact walls (neighboring tiles are walls)
                    if (HasThreeWallsIntact(toCheck))
                    {
                        validNeighbors.Add(toCheck);
                    }
                }
            }
        }

        return validNeighbors;
    }

    private bool IsInside(Vector2 p)
    {
        // Ensure that the point is within the bounds of the maze
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
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
}
