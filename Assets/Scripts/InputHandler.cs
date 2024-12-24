using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private Vector2Int currentDirection = Vector2Int.up;
    private Vector2Int lastExecutedDirection = Vector2Int.up;

    public void SetDirection(Vector2Int direction)
    {
        currentDirection = direction;
        lastExecutedDirection = direction;
    }

    public Vector2Int GetLastDirection()
    {
        return lastExecutedDirection;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && lastExecutedDirection != Vector2Int.down)
            currentDirection = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) && lastExecutedDirection != Vector2Int.up)
            currentDirection = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) && lastExecutedDirection != Vector2Int.right)
            currentDirection = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) && lastExecutedDirection != Vector2Int.left)
            currentDirection = Vector2Int.right;
    }

    public Vector2Int GetCurrentDirection()
    {
        Vector2Int directionToReturn = currentDirection;
        lastExecutedDirection = currentDirection;
        return directionToReturn;
    }
}