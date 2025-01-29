using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class MazeGenerator3 : MonoBehaviour
{
    public int width, height;
    public TileBase wallTile; // Reference to the wall tile
    public Tilemap tilemap; // Reference to the Tilemap for walls

    private int[,] Maze;
    private Stack<Vector2> _tiletoTry = new Stack<Vector2>(); // Stack for tiles to try
    private List<Vector2> offsets = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    private System.Random rnd = new System.Random();
    private Vector2 entrance;
    private Vector2 exit;

    public TileBase floorTile; // Reference to the floor tile
    public Tilemap floorTilemap; // Reference to the Tilemap for floors

    // Declare CurrentTile here as a class member
    private Vector2 CurrentTile;

    public GameObject player; // Reference to the Player GameObject
    public Camera minimapCamera;

    public EnemyAI enemyAI; // Reference to the EnemyAI script


    // public RawImage minimapRawImage;

    void Start()
    {
        Camera.main.orthographic = true; // Ensure the camera is orthographic
        minimapCamera.orthographic = true;
        AdjustCameraToMaze(); // Adjust camera size and position
        AdjustMiniMApCameraToMaze();
        adjustRawImage();
        tilemap.ClearAllTiles(); // Clear any existing tiles in the Tilemap
        GenerateMaze(); // Generate the maze
        SetFloorTiles();
        PlacePlayerAtEntrance();
        BlockMazeEntrance();

        // Place the enemy at the entrance
        PlaceEnemyAtEntrance(enemyAI.gameObject);

        // Delay enemy detection for 5 seconds
        StartCoroutine(DelayEnemyDetection(5f));

        if (enemyAI != null)
        {
            enemyAI.maze = Maze; // Assign the maze array
            enemyAI.tilemap = floorTilemap; // Assign the tilemap
            enemyAI.player = player.transform; // Assign the player's transform
        }
        else
        {
            Debug.LogError("EnemyAI reference is not assigned!");
        }
    }
    private IEnumerator DelayEnemyDetection(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Enable the enemy's collider and start detection
        if (enemyAI != null)
        {
            enemyAI.EnableCollider();
        }
    }

    private void PlaceEnemyAtEntrance(GameObject enemy)
    {
        if (enemy != null)
        {
            // Convert entrance coordinates to world position
            Vector3 worldPosition = floorTilemap.CellToWorld(new Vector3Int((int)entrance.x, (int)entrance.y, 0));

            // Slightly adjust the position to center the enemy on the tile
            enemy.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            Debug.LogError("Enemy GameObject is not assigned!");
        }
    }

    private void adjustRawImage()
    {
       // minimapRawImage.transform.localScale = new Vector2(width / 25, height / 25);
    }
    private void SetFloorTiles()
    {
        // Clear any existing tiles on the floor tilemap
        floorTilemap.ClearAllTiles();

        // Iterate through the maze array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // If the maze tile is a floor (0), set the floor tile
                if (Maze[x, y] == 0)
                {
                    floorTilemap.SetTile(tilePosition, floorTile);
                }
            }
        }

    }
    private void BlockMazeEntrance()
    {
        // Get or add the EdgeCollider2D
        EdgeCollider2D edgeCollider = floorTilemap.GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = floorTilemap.gameObject.AddComponent<EdgeCollider2D>();
        }

        // Convert the entrance position to world space
        Vector3 entranceWorldPos = tilemap.CellToWorld(new Vector3Int((int)entrance.x, (int)entrance.y, 0));

        // Define the points for the edge to block the entrance
        Vector2[] edgePoints = new Vector2[]
        {
        new Vector2(entranceWorldPos.x, entranceWorldPos.y),
        new Vector2(entranceWorldPos.x + 1f, entranceWorldPos.y) // Adjust width as needed
        };

        // Set the points for the EdgeCollider2D
        edgeCollider.points = edgePoints;
    }


    private void PlacePlayerAtEntrance()
    {
        if (player != null)
        {
            // Convert entrance coordinates to world position
            Vector3 worldPosition = tilemap.CellToWorld(new Vector3Int((int)entrance.x, (int)entrance.y, 0));

            // Slightly adjust the position to center the player on the tile
            player.transform.position = worldPosition + new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            Debug.LogError("Player GameObject is not assigned!");
        }
    }

    void AdjustMiniMApCameraToMaze()
    {
        // Calculate the orthographic size to fit the maze height (add half-tile offset)
        minimapCamera.orthographicSize = (height / 2f) + 5f;

        // Calculate the aspect ratio of the camera (width/height)
        float screenAspect = (float)Screen.width / Screen.height;

        // Calculate the camera width based on the aspect ratio and orthographic size
        float cameraWidth = minimapCamera.orthographicSize * screenAspect;

        // If the maze width is larger than the camera width, adjust the orthographic size
        if (cameraWidth < (width / 2f) + 0.5f)
        {
            minimapCamera.orthographicSize = ((width / 2f) + 0.5f) / screenAspect;
        }

        // Center the camera on the maze (considering half-tile offsets)
        minimapCamera.transform.position = new Vector3((width / 2f) - 0.1f, (height / 2f) - 0.1f, -10f);

    }


    void AdjustCameraToMaze()
    {
        // Calculate the orthographic size to fit the maze height (add half-tile offset)
        Camera.main.orthographicSize = (height / 5f) + 1f;

        // Calculate the aspect ratio of the camera (width/height)
        float screenAspect = (float)Screen.width / Screen.height;

        // Calculate the camera width based on the aspect ratio and orthographic size
        float cameraWidth = Camera.main.orthographicSize * screenAspect;

        // If the maze width is larger than the camera width, adjust the orthographic size
        if (cameraWidth < (width / 1f) + 1f)
        {
            Camera.main.orthographicSize = ((width / 3f) + 0.5f) / screenAspect;
        }

        // Center the camera on the maze (considering half-tile offsets)
        Camera.main.transform.position = new Vector3((width / 2f) - 0.5f, (height / 2f) - 0.5f, -10f);
        Camera.main.transform.position = new Vector3(player.gameObject.transform.position.x, player.gameObject.transform.position.y, Camera.main.transform.position.z);
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

        if (floorTileCount > 2)
        {
            Maze[1, 0] = 1; // Replace start tile with wall
        }
    }

    private void SetEntranceAndExit()
    {
        if (rnd.Next(2) == 0)
        {
            entrance = new Vector2(1, 0);  // Top-left corner
            exit = new Vector2(width - 2, height - 1);  // Bottom-right corner
        }
        else
        {
            entrance = new Vector2(width - 2, 0);  // Top-right corner
            exit = new Vector2(1, height - 1);  // Bottom-left corner
        }

        Maze[(int)entrance.x, (int)entrance.y] = 0;
        Maze[(int)exit.x, (int)exit.y] = 0;

        Debug.Log("Entrance: " + entrance);
        Debug.Log("Exit: " + exit);
    }

    public int[,] CreateMaze()
    {
        List<Vector2> neighbors;
        CurrentTile = new Vector2(1, 0); // Initialize CurrentTile
        _tiletoTry.Push(CurrentTile);

        while (_tiletoTry.Count > 0)
        {
            Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;

            neighbors = GetValidNeighbors(CurrentTile);

            if (neighbors.Count > 0)
            {
                _tiletoTry.Push(CurrentTile);
                CurrentTile = neighbors[rnd.Next(neighbors.Count)];
                _tiletoTry.Push(CurrentTile);
                Maze[(int)CurrentTile.x, (int)CurrentTile.y] = 0;
            }
            else
            {
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

            if (IsInside(toCheck))
            {
                if ((toCheck.x % 2 == 1 || toCheck.y % 2 == 1) && Maze[(int)toCheck.x, (int)toCheck.y] == 1)
                {
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

