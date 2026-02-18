using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows;
    public int colomns = 10;
    public float cellSpacing = 1f;
    public List<Cell> allCells = new List<Cell>();
    public List<Cell> topRowCells = new List<Cell>();
    public static GridManager instance;
    void Start()
    {
      //  GenerateGrid();
        //StartCoroutine(MoveGridDownRoutine());
    }
    private void Awake()
    {
        instance = this;
    }
    //IEnumerator MoveGridDownRoutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(5f);

    //        MoveGridDown();
    //        AddNewRow();
    //    }
    //}
    //void MoveGridDown()
    //{
    //    foreach (Cell c in allCells)
    //    {
    //        if (c != null && c.gameObject.activeInHierarchy)
    //        {
    //            c.transform.position += Vector3.down * cellSpacing;
    //        }
    //    }
    //}
    //void AddNewRow()
    //{
    //    Camera cam = Camera.main;
    //    float topY = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

    //    List<Cell> newRow = new List<Cell>();

    //    for (int j = 0; j < colomns; j++)
    //    {
    //        var cellObj = CellPooler.instance.GetCell();

    //        if (cellObj == null)
    //        {
    //            Debug.LogError("CellPooler returned null! Check if prefab is assigned and pool has enough cells.");
    //            return;
    //        }
    //        cellObj.layer = LayerMask.NameToLayer("GridCell");

    //        var cell = cellObj.GetComponent<Cell>();
    //        CellColor randomColor = (CellColor)Random.Range(0, 3);
    //        cell.setColor(randomColor);

    //        allCells.Add(cell);
    //        newRow.Add(cell);

    //        Vector2 pos = new Vector2(
    //            j * cellSpacing - (colomns - 1) * cellSpacing * 0.5f,
    //            topY - cellSpacing * 0.5f
    //        );
    //        cellObj.transform.position = pos;
    //    }


    //    topRowCells.Clear();
    //    topRowCells.AddRange(newRow);
    //}

    public void GenerateGrid()
    {
        rows = DifficultyManager.Instance.GetRowCount();

        Camera cam = Camera.main;
        float topY = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        if (CellPooler.instance == null)
        {
            return;
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < colomns; j++)
            {
                var cellObj = CellPooler.instance.GetCell();
                cellObj.layer = LayerMask.NameToLayer("GridCell");

                var cell = cellObj.GetComponent<Cell>();
                int allowedColors = DifficultyManager.Instance.GetAllowedColorCount();

                CellColor randomColor = (CellColor)Random.Range(0, allowedColors);

                cell.setColor(randomColor);
                allCells.Add(cell);

                if (i == 0)
                {
                    topRowCells.Add(cell);
                }
;
                Vector2 pos = new Vector2(j * cellSpacing - (colomns - 1) * cellSpacing * 0.5f, topY - i * cellSpacing - cellSpacing * 0.5f);
                cellObj.transform.position = pos;
            }
        }
    }
    public void CheckFloatingCells()
    {
        HashSet<Cell> connectedToTop = new HashSet<Cell>();

        Queue<Cell> toCheck = new Queue<Cell>();


        foreach (Cell top in topRowCells)
        {
            if (top.gameObject.activeInHierarchy)
            {
                toCheck.Enqueue(top);
                connectedToTop.Add(top);
            }
        }

        while (toCheck.Count > 0)
        {
            Cell current = toCheck.Dequeue();

            List<Cell> neighbors = current.GetConnectedCells();

            foreach (Cell neighbor in neighbors)
            {
                if (!connectedToTop.Contains(neighbor))
                {
                    connectedToTop.Add(neighbor);
                    toCheck.Enqueue(neighbor);
                }
            }
        }


        List<Cell> floating = new List<Cell>();

        foreach (Cell c in allCells)
        {
            if (!connectedToTop.Contains(c))
            {
                floating.Add(c);
            }
        }

        foreach (Cell c in floating)
        {
            allCells.Remove(c);
            topRowCells.Remove(c);
            c.gameObject.layer = LayerMask.NameToLayer("Cell");
            c.FallAndDisable();
        }

    }

}
