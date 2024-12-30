using Zenject;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    private Vector2Int snakeHead;
    public List<Vector2Int> SnakeBody { get; private set; } = new List<Vector2Int>();
    [Inject] private GameSettings settings;
    [Inject] private GridSystem gridSystem;
    [Inject] private InputHandler inputHandler;
    [Inject] private ItemManager itemManager;
    [Inject] private UIManager uiManager;
    //hard coded strings
    [Inject(Id = "SnakeSegment")] private GameObject SnakeSegmentPrefab;
    [Inject(Id = "CenterIndicator")] private GameObject CenterIndicatorPrefab;

    private ObjectPool segmentPool;
    private List<GameObject> snakeSegments = new List<GameObject>();
    private List<Vector2Int> dissolvingSegments = new List<Vector2Int>();
    private List<GameObject> dissolvingObjects = new List<GameObject>();
    private Coroutine dissolutionCoroutine;
    private GameObject centerIndicator;
    private float lastMoveTime = 0f;
    private bool isGameOver = false;

    public void Initialize()
    {
        segmentPool = gameObject.AddComponent<ObjectPool>();
        segmentPool.Initialize(SnakeSegmentPrefab, 40, transform);
    }

    public void UpdateSnake()
    {
        if (isGameOver) return;

        if (Time.time - lastMoveTime >= settings.SnakeMoveInterval)
        {
            MoveSnake(inputHandler.GetCurrentDirection());
            lastMoveTime = Time.time;
        }
    }

    public void LoadGameState(List<Vector2Int> savedSnakeBody)
    {
        ResetSnake(false);

        foreach (Vector2Int pos in savedSnakeBody)
        {
            SnakeBody.Add(pos);
            gridSystem.SetCell(pos, GridCell.Snake);

            GameObject segment = segmentPool.GetObject();
            segment.transform.position = new Vector3(pos.x, pos.y, 0);
            snakeSegments.Add(segment);
        }

        if (savedSnakeBody.Count > 0)
        {
            snakeHead = savedSnakeBody[0];
        }

        EnsureCenterIndicator();
        isGameOver = false;
        lastMoveTime = Time.time;
    }

    private void EnsureCenterIndicator()
    {
        if (centerIndicator == null && CenterIndicatorPrefab != null)
        {
            centerIndicator = Instantiate(CenterIndicatorPrefab, Vector3.zero, Quaternion.identity);
            if (this != null && this.gameObject != null)
            {
                centerIndicator.transform.SetParent(transform);
            }
            UpdateCenterIndicator();
        }
    }

    public void ResetSnake(bool placeNewSnake = true)
    {
        foreach (var segment in snakeSegments)
        {
            if (segment != null)
            {
                segmentPool.ReturnObject(segment);
            }
        }

        if (centerIndicator != null)
        {
            Destroy(centerIndicator);
            centerIndicator = null;
        }

        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
        }

        snakeSegments.Clear();
        SnakeBody.Clear();
        dissolvingSegments.Clear();
        dissolvingObjects.Clear();
        isGameOver = false;

        if (placeNewSnake)
        {
            PlaceSnake();
        }
    }

    private void PlaceSnake()
    {
        var (rows, cols) = gridSystem.GetGridDimensions();
        snakeHead = new Vector2Int(cols / 2, rows / 2);
        gridSystem.SetCell(snakeHead, GridCell.Snake);
        SnakeBody.Add(snakeHead);

        GameObject headSegment = segmentPool.GetObject();
        headSegment.transform.position = new Vector3(snakeHead.x, snakeHead.y, 0);
        snakeSegments.Add(headSegment);

        EnsureCenterIndicator();
    }

    private void MoveSnake(Vector2Int direction)
    {
        Vector2Int newHeadPos = snakeHead + direction;

        if (gridSystem.IsWallCollision(newHeadPos))
        {
            EndGame();
            return;
        }

        if (IsTailCollision(newHeadPos))
        {
            HandleSnakeDivision(newHeadPos);
            return;
        }

        bool isItem = gridSystem.GetCell(newHeadPos) == GridCell.Item;

        gridSystem.SetCell(snakeHead, GridCell.Empty);
        snakeHead = newHeadPos;
        SnakeBody.Insert(0, snakeHead);
        gridSystem.SetCell(snakeHead, GridCell.Snake);

        GameObject newSegment = segmentPool.GetObject();
        newSegment.transform.position = new Vector3(snakeHead.x, snakeHead.y, 0);
        snakeSegments.Insert(0, newSegment);

        UpdateCenterIndicator();

        if (isItem)
        {
            itemManager.CollectItem(newHeadPos);
        }
        else
        {
            RemoveTail();
        }
        
        itemManager.UpdateSnakePositions(SnakeBody);
    }

    private void UpdateCenterIndicator()
    {
        if (SnakeBody.Count == 0 || CenterIndicatorPrefab == null) return;
    
        if (centerIndicator == null)
        {
            centerIndicator = Instantiate(CenterIndicatorPrefab, Vector3.zero, Quaternion.identity);
            if (this != null && this.gameObject != null)
            {
                centerIndicator.transform.SetParent(transform);
            }
        }
    
        Vector2Int centerPosition;
        if (SnakeBody.Count < 3)
        {
            centerPosition = SnakeBody[0];
        }
        else
        {
            int centerIndex = (SnakeBody.Count - 1) / 2;
            centerPosition = SnakeBody[centerIndex];
        }
    
        if (centerIndicator != null)
        {
            centerIndicator.transform.position = new Vector3(centerPosition.x, centerPosition.y, 0);
        }
    }

    private bool IsTailCollision(Vector2Int position)
    {
        return SnakeBody.Contains(position);
    }

    private void HandleSnakeDivision(Vector2Int collisionPoint)
    {
        int collisionIndex = SnakeBody.IndexOf(collisionPoint);
        int totalLength = SnakeBody.Count;
        int frontLength = collisionIndex;
        int backLength = totalLength - collisionIndex;

        float frontPercentage = (float)frontLength / totalLength * 100;
        float backPercentage = (float)backLength / totalLength * 100;

        if (frontPercentage > 50)
        {
            EndGame();
        }
        else if (backPercentage > 50)
        {
            List<Vector2Int> backSection = new List<Vector2Int>(SnakeBody.GetRange(collisionIndex, backLength));
            List<GameObject> backObjects = new List<GameObject>(snakeSegments.GetRange(collisionIndex, backLength));
            StartDissolution(backSection, backObjects);

            SnakeBody.RemoveRange(collisionIndex, backLength);
            snakeSegments.RemoveRange(collisionIndex, backLength);
        }
    }

    private void StartDissolution(List<Vector2Int> segmentsToDissolve, List<GameObject> objectsToDissolve)
    {
        dissolvingSegments = new List<Vector2Int>(segmentsToDissolve);
        dissolvingObjects = new List<GameObject>(objectsToDissolve);

        foreach (var segment in dissolvingSegments)
        {
            gridSystem.SetCell(segment, GridCell.Empty);
        }

        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
        }
        dissolutionCoroutine = StartCoroutine(DissolutionRoutine());
    }

    private IEnumerator DissolutionRoutine()
    {
        int index = dissolvingSegments.Count - 1;

        while (index >= 0)
        {
            yield return new WaitForSeconds(settings.DissolutionInterval);

            Vector2Int segment = dissolvingSegments[index];
            gridSystem.SetCell(segment, GridCell.Empty);

            if (index < dissolvingObjects.Count)
            {
                GameObject obj = dissolvingObjects[index];
                if (obj != null)
                {
                    segmentPool.ReturnObject(obj);
                }
            }

            index--;
        }

        dissolvingSegments.Clear();
        dissolvingObjects.Clear();
    }

    private void RemoveTail()
    {
        Vector2Int tail = SnakeBody[SnakeBody.Count - 1];
        SnakeBody.RemoveAt(SnakeBody.Count - 1);
        gridSystem.SetCell(tail, GridCell.Empty);

        GameObject tailSegment = snakeSegments[snakeSegments.Count - 1];
        snakeSegments.RemoveAt(snakeSegments.Count - 1);
        segmentPool.ReturnObject(tailSegment);
    }

    private void EndGame()
    {
        isGameOver = true;
        itemManager.Stop();

        if (centerIndicator != null)
        {
            Destroy(centerIndicator);
            centerIndicator = null;
        }

        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
        }

        if (uiManager != null)
        {
            uiManager.EndGame();
        }

        Debug.Log("Game Over!");
    }

    public Vector2Int GetHeadPosition()
    {
        return snakeHead;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public List<Vector2Int> GetSnakePositions()
    {
        return new List<Vector2Int>(SnakeBody);
    }

    public void SetGameOver(bool state)
    {
        isGameOver = state;
        if (state)
        {
            itemManager.Stop();
            if (centerIndicator != null)
            {
                Destroy(centerIndicator);
                centerIndicator = null;
            }
        }
    }
}