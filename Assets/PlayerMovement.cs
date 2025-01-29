using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // Speed of the player movement

    void Update()
    {
        // Get input from arrow keys or WASD (Horizontal and Vertical axes)
        float moveX = Input.GetAxis("Horizontal"); // Left/Right movement
        float moveY = Input.GetAxis("Vertical");   // Up/Down movement

        // Combine inputs into a movement vector
        Vector2 movement = new Vector2(moveX, moveY);

        // Apply movement to the GameObject
        transform.Translate(movement * speed * Time.deltaTime);
    }
}
