using Zenject;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("Item Settings")]
    private List<GameObject> activeItems = new List<GameObject>();
    private List<Vector2Int> itemPositions = new List<Vector2Int>();
    private Coroutine itemSpawnCoroutine;


    [Inject] private GameSettings settings;
    [Inject] private ScoreSystem scoreSystem;
    [Inject] private GridSystem gridSystem;
    [Inject] private SnakeController snakeController;
    [Inject(Id = "Item")] private GameObject itemPrefab;
    
    private List<Vector2Int> snakePositions;

    public void Initialize()
    {
        snakePositions = snakeController.SnakeBody;
        StartSpawning();
    }

    private void StartSpawning()
    {
        if (itemSpawnCoroutine != null)
        {
            StopCoroutine(itemSpawnCoroutine);
        }
        itemSpawnCoroutine = StartCoroutine(ItemSpawnRoutine());
    }

    private IEnumerator ItemSpawnRoutine()
    {
        SpawnItem();
       
        while (true)
        {
            yield return new WaitForSeconds(settings.ItemSpawnInterval);
           
            if (activeItems.Count < settings.MaxItems)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        if (activeItems.Count >= settings.MaxItems) return;

        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        HashSet<Vector2Int> snakeSet = new HashSet<Vector2Int>(snakePositions);
        HashSet<Vector2Int> existingItemPositions = new HashSet<Vector2Int>(itemPositions);
        var (rows, cols) = gridSystem.GetGridDimensions();

        for (int r = 1; r < rows - 1; r++)
        {
            for (int c = 1; c < cols - 1; c++)
            {
                Vector2Int pos = new Vector2Int(c, r);
                if (gridSystem.GetCell(pos) == GridCell.Empty && 
                    !snakeSet.Contains(pos) && 
                    !existingItemPositions.Contains(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        if (emptyPositions.Count > 0)
        {
            Vector2Int spawnPos = emptyPositions[Random.Range(0, emptyPositions.Count)];
            SpawnItemAt(spawnPos);
        }
    }

    public void SpawnItemAt(Vector2Int spawnPos)
    {
        if (activeItems.Count >= settings.MaxItems) return;

        gridSystem.SetCell(spawnPos, GridCell.Item);
        GameObject newItem = Instantiate(itemPrefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity, transform);
        
        activeItems.Add(newItem);
        itemPositions.Add(spawnPos);
    }

    public void CollectItem(Vector2Int itemPosition)
    {
        for (int i = 0; i < itemPositions.Count; i++)
        {
            if (itemPositions[i] == itemPosition)
            {
                gridSystem.SetCell(itemPosition, GridCell.Empty);
                Destroy(activeItems[i]);
                activeItems.RemoveAt(i);
                itemPositions.RemoveAt(i);
                break;
            }
        }

        scoreSystem.AddScore();
    }

    public void UpdateSnakePositions(List<Vector2Int> newPositions)
    {
        snakePositions = newPositions;
    }

    public void Stop()
    {
        if (itemSpawnCoroutine != null)
        {
            StopCoroutine(itemSpawnCoroutine);
            itemSpawnCoroutine = null;
        }
    }

    public void Reset()
    {
        Stop();
        ClearAllItems();
    }

    public void ClearAllItems()
    {
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        activeItems.Clear();
        itemPositions.Clear();
    }

    public bool IsItemPosition(Vector2Int position)
    {
        return itemPositions.Contains(position);
    }

    public List<Vector2Int> GetAllItemPositions()
    {
        return new List<Vector2Int>(itemPositions);
    }
}