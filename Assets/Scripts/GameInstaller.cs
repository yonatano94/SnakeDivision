using Zenject;
using UnityEngine;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject snakeSegmentPrefab;
    [SerializeField] private GameObject centerIndicatorPrefab;

    public override void InstallBindings()
    {
        Container.Bind<GameObject>().WithId("Item").FromInstance(itemPrefab);
        Container.Bind<GameObject>().WithId("Wall").FromInstance(wallPrefab);
        Container.Bind<GameObject>().WithId("SnakeSegment").FromInstance(snakeSegmentPrefab);
        Container.Bind<GameObject>().WithId("CenterIndicator").FromInstance(centerIndicatorPrefab);
        
        Container.Bind<GameSettings>().AsSingle();
        Container.Bind<ScoreSystem>().AsSingle();
        Container.Bind<UIManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GridManager>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<SaveSystem>().FromNewComponentOnNewGameObject().WithGameObjectName("SaveSystem").AsSingle();
        Container.Bind<GridSystem>().FromNewComponentOnNewGameObject().WithGameObjectName("GridSystem").AsSingle();
        Container.Bind<ItemManager>().FromNewComponentOnNewGameObject().WithGameObjectName("ItemManager").AsSingle();
        Container.Bind<InputHandler>().FromNewComponentOnNewGameObject().WithGameObjectName("InputHandler").AsSingle();
        Container.Bind<SnakeController>().FromNewComponentOnNewGameObject().WithGameObjectName("SnakeController").AsSingle();
    }
}