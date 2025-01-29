using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float moveSpeed = 2f; // Enemy movement speed
    public int[,] maze; // Reference to the maze structure
    public Tilemap tilemap; // Reference to the maze Tilemap for positions

    private Vector2Int currentTarget; // The tile the enemy is moving toward
    private Queue<Vector2Int> pathToPlayer = new Queue<Vector2Int>(); // Path for enemy to follow
    private List<Vector2Int> offsets = new List<Vector2Int> // Movement directions
    {
        new Vector2Int(0, 1),  // Up
        new Vector2Int(0, -1), // Down
        new Vector2Int(1, 0),  // Right
        new Vector2Int(-1, 0)  // Left
    };
    private Collider2D enemyCollider; // Reference to the enemy's collider
    public bool canDetectPlayer = false; // Flag to start detection

    void Update()
    {
        if (canDetectPlayer && pathToPlayer.Count > 0)
        {
            // Move toward the next target tile
            Vector2 nextPosition = tilemap.CellToWorld((Vector3Int)pathToPlayer.Peek()) + new Vector3(0.5f, 0.5f, 0);
            transform.position = Vector2.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            // Check if the enemy has reached the current target
            if (Vector2.Distance(transform.position, nextPosition) < 0.1f)
            {
                pathToPlayer.Dequeue(); // Move to the next tile in the path
            }
        }
    }
    public void EnableCollider()
    {
        // Enable the collider after the delay
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
            canDetectPlayer = true; // Start detecting the player
            Debug.Log("Enemy collider enabled. Detection started!");
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player is not assigned!");
            return;
        }

        // Start following the player
        InvokeRepeating(nameof(UpdatePathToPlayer), 0f, 0.5f); // Update path every 0.5 seconds

        // Get the collider component
        enemyCollider = GetComponent<Collider2D>();

        // Disable the collider at the start
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }


    private void UpdatePathToPlayer()
    {
        Vector3Int playerCell = tilemap.WorldToCell(player.position);
        Vector3Int enemyCell = tilemap.WorldToCell(transform.position);

        Vector2Int start = new Vector2Int(enemyCell.x, enemyCell.y);
        Vector2Int end = new Vector2Int(playerCell.x, playerCell.y);

        Debug.Log($"Finding path from {start} to {end}");

        // Ensure both start and end are walkable
        if (IsWalkable(start) && IsWalkable(end))
        {
            pathToPlayer = FindPath(start, end);
            //Debug.Log($"Path found with {pathToPlayer.Count} steps");
        }
        else
        {
            //Debug.LogError("Start or End position is not walkable!");
        }
    }

    private Queue<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        const int maxExploredTiles = 1000; // Adjust this value as needed
        Queue<Vector2Int> path = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        frontier.Enqueue(start);
        visited.Add(start);

        int exploredTiles = 0;

        while (frontier.Count > 0)
        {
            if (exploredTiles > maxExploredTiles)
            {
               // Debug.LogError("Exceeded maximum explored tiles. Pathfinding aborted.");
                break;
            }

            Vector2Int current = frontier.Dequeue();

            if (current == end)
            {
                break;
            }

            foreach (Vector2Int offset in offsets)
            {
                Vector2Int neighbor = current + offset;

                if (IsWalkable(neighbor) && !visited.Contains(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    exploredTiles++;
                }
            }
        }

        if (!cameFrom.ContainsKey(end))
        {
            Debug.LogError("No valid path found!");
            return path;
        }

        Vector2Int currentPathTile = end;
        while (currentPathTile != start)
        {
            path.Enqueue(currentPathTile);
            currentPathTile = cameFrom[currentPathTile];
        }

        path.Enqueue(start);
        return new Queue<Vector2Int>(path.Reverse());
    }


    private bool IsWalkable(Vector2Int position)
    {
        // Check bounds
        if (position.x < 0 || position.y < 0 || position.x >= maze.GetLength(0) || position.y >= maze.GetLength(1))
        {
            Debug.Log($"Position {position} is out of bounds.");
            return false;
        }

        // Check walkability (maze value should be 0 for walkable)
        if (maze[position.x, position.y] != 0)
        {
            Debug.Log($"Position {position} is not walkable.");
            return false;
        }

        return true;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Game over if the enemy collides with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Over! Enemy caught the player.");
            // Pause the game or trigger Game Over logic
            Time.timeScale = 0;
        }
    }
}
