public class GameSettings
{
    public string SaveFileName { get; } = "snakegame.json";
    public float SnakeMoveInterval { get; } = 0.07f;
    public float DissolutionInterval { get; } = 0.2f;
    public float ItemSpawnInterval { get; } = 3f;
    public int MaxItems { get; } = 2;
    public int InitialPoolSize { get; } = 40;
    public int PointsPerItem { get; } = 10;
    public int GridRows { get; } = 20;
    public int GridCols { get; } = 20;
}