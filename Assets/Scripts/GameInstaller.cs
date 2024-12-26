using Zenject;
using UnityEngine;


//this installer can be divided into smaller ones  
//missing namespace 
public class GameInstaller : MonoInstaller
{
    //prefabs live in the project scope and should use ScriptableInstallers instead of MonoInstaller
    //
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject snakeSegmentPrefab;
    [SerializeField] private GameObject centerIndicatorPrefab;

    public override void InstallBindings()
    {
        //hard coded strings
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