using System.Collections.Generic;
using UnityEngine;

public class CellPooler : MonoBehaviour
{
    public static CellPooler instance;
    public GameObject cellPrefab;
    public int cellPoolSize = 100;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {

        instance = this;
        for (int i = 0; i < cellPoolSize; i++)
        {
            var cellObj = Instantiate(cellPrefab);
            cellObj.SetActive(false);
            pool.Enqueue(cellObj);
        }
    }

    public GameObject GetCell()
    {
        if (pool.Count == 0)
        {
            return null;
        }

        var cellObj = pool.Dequeue();
        cellObj.SetActive(true);
        return cellObj;
    }

    public void ReturnCell(GameObject cellObj)
    {
        cellObj.SetActive(false);
        pool.Enqueue(cellObj);
    }
}
