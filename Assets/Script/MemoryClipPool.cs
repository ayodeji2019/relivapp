using System.Collections.Generic;
using UnityEngine;

public class MemoryClipPool : MonoBehaviour
{
    public GameObject memoryClipPrefab;
    public int poolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // Initialize the pool with inactive memory clips
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(memoryClipPrefab);
            obj.SetActive(false);
            obj.transform.SetParent(this.transform); // Set parent to the pool's transform
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Retrieves a memory clip from the pool and sets its parent to the specified transform.
    /// </summary>
    public GameObject GetPooledObject(Transform parent)
{
    GameObject obj;
    if (pool.Count > 0)
    {
        obj = pool.Dequeue();
    }
    else
    {
        obj = Instantiate(memoryClipPrefab);
    }
    obj.SetActive(true);
    obj.transform.SetParent(parent, false); 
    return obj;
}

public void ReturnToPool(GameObject obj)
{
    obj.SetActive(false);
    obj.transform.SetParent(this.transform, false); 
    pool.Enqueue(obj);
}

}
