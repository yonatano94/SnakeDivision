using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private GameObject prefab;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private Transform parent;

    public void Initialize(GameObject prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private void CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        availableObjects.Enqueue(obj);
    }

    public GameObject GetObject()
    {
        if (availableObjects.Count == 0)
        {
            CreateNewObject();
        }

        GameObject obj = availableObjects.Dequeue();
        obj.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null || !activeObjects.Contains(obj))
            return;

        obj.SetActive(false);
        activeObjects.Remove(obj);
        availableObjects.Enqueue(obj);
    }

    public void ClearPool()
    {
        foreach (var obj in availableObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        availableObjects.Clear();

        foreach (var obj in activeObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        activeObjects.Clear();
    }

    public bool IsObjectActive(GameObject obj)
    {
        return activeObjects.Contains(obj);
    }
}