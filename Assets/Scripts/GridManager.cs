using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 3;
    public int colomns = 10;
    public float cellSpacing = 1f;
    public List<Cell> allCells = new List<Cell>();
    public List<Cell> topRowCells = new List<Cell>();

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
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
                var cell = cellObj.GetComponent<Cell>();
                CellColor randomColor = (CellColor)Random.Range(0, 3);
                cell.setColor(randomColor);
                    allCells.Add(cell);

                if (i == 0) // top row
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

        // Start BFS from all top row cells
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

        // Any cell NOT connected to top falls
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

            c.FallAndDisable();
        }

    }

}
