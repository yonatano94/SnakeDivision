using Zenject;
using UnityEngine;

public enum GridCell
{
    Empty = 0,
    Snake = 1,
    Item = 2,
    Wall = 3
}

public class GridSystem : MonoBehaviour
{
    private GridCell[,] grid;
    [Inject] private GameSettings settings;
    [Inject(Id = "Wall")] private GameObject wallPrefab;

    public void Initialize()
    {
        InitializeGrid();
        PlaceWalls();
    }

    private void InitializeGrid()
    {
        grid = new GridCell[settings.GridRows, settings.GridCols];
        for (int r = 0; r < settings.GridRows; r++)
        {
            for (int c = 0; c < settings.GridCols; c++)
            {
                grid[r, c] = GridCell.Empty;
            }
        }
    }

    private void PlaceWalls()
    {
        for (int r = 0; r < settings.GridRows; r++)
        {
            PlaceWall(r, 0);
            PlaceWall(r, settings.GridCols - 1);
        }
        for (int c = 0; c < settings.GridCols; c++)
        {
            PlaceWall(0, c);
            PlaceWall(settings.GridRows - 1, c);
        }
    }

    private void PlaceWall(int row, int col)
    {
        grid[row, col] = GridCell.Wall;
        Instantiate(wallPrefab, new Vector3(col, row, 0), Quaternion.identity, transform);
    }

    public bool IsWallCollision(Vector2Int position)
    {
        return position.x < 0 || position.x >= settings.GridCols || 
               position.y < 0 || position.y >= settings.GridRows || 
               grid[position.y, position.x] == GridCell.Wall;
    }

    public void SetCell(Vector2Int position, GridCell value)
    {
        if (IsValidPosition(position))
        {
            grid[position.y, position.x] = value;
        }
    }

    public GridCell GetCell(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return grid[position.y, position.x];
        }
        return GridCell.Wall;
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < settings.GridCols && 
               position.y >= 0 && position.y < settings.GridRows;
    }

    public void Reset()
    {
        InitializeGrid();
        PlaceWalls();
    }

    public (int rows, int cols) GetGridDimensions()
    {
        return (settings.GridRows, settings.GridCols);
    }
}