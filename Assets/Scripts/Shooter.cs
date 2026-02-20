using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Shooter : MonoBehaviour
{
    public static Shooter Instance;
    public Transform shootPoint;
    public LayerMask CellLayer;
    public float rotateLimit = 90f;
    public float shootSpeed = 10f;
    private GameObject currentCell;
    private Collider2D currentCellCollider;
    private LineRenderer lineRenderer;
    private bool isShooting = false;
    public LayerMask WallLayer;
    public LayerMask TopWallLayer;
    public int score = 0;
    private bool canShoot = false;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.sortingOrder = 10;
        SpawnNewCell();
    }

    //void Update()
    //{
    //    if (isShooting) return;
    //    AimAtMouse();
    //    DrawAimingLine();
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        StartCoroutine(ShootCellAlongRay());
    //    }
    //}

    void Update()
    {
        if (!canShoot || isShooting) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        AimAtPosition(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(ShootCellAlongRay());
        }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        AimAtPosition(touch.position);

        if (touch.phase == TouchPhase.Ended)
        {
            StartCoroutine(ShootCellAlongRay());
        }
    }
#endif

        DrawAimingLine();
    }
    public void SpawnNewCell()
    {
        currentCell = CellPooler.instance.GetCell();
        var cell = currentCell.GetComponent<Cell>();
        int allowedColors = DifficultyManager.Instance.GetAllowedColorCount();
        CellColor randomColor = (CellColor)Random.Range(0, allowedColors);
        cell.setColor(randomColor);
        currentCell.transform.position = shootPoint.position;
        currentCell.SetActive(true);
        currentCellCollider = currentCell.GetComponent<Collider2D>();
    }
    //public void AimAtMouse()
    // {
    //     Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     mousePos.z = 0;
    //     Vector2 direction = (mousePos - transform.position).normalized;
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    //     angle = Mathf.Clamp(angle, -rotateLimit, rotateLimit);
    //     transform.rotation = Quaternion.Euler(0, 0, angle);
    // }
    public void AimAtPosition(Vector3 inputPosition)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(inputPosition);
        worldPos.z = 0;

        Vector2 direction = (worldPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        angle = Mathf.Clamp(angle, -rotateLimit, rotateLimit);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void EnableShooting()
    {
        StartCoroutine(EnableShootDelay());
    }

    IEnumerator EnableShootDelay()
    {
        yield return new WaitForSeconds(0.2f);
        canShoot = true;
    }
    public void DisableShooting()
    {
        StartCoroutine(DisableShootDelay());
    }

    IEnumerator DisableShootDelay()
    {
        yield return new WaitForSeconds(0.2f);
        canShoot = false;
    }

    public void DrawAimingLine()
    {
        Vector2 origin = shootPoint.position;
        Vector2 dir = shootPoint.up;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, origin);
        int maxBounces = 4;
        float remaining = 50f;
        int oldLayer = currentCell.layer;
        currentCell.layer = LayerMask.NameToLayer("Ignore Raycast");
        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast( origin, dir, remaining, CellLayer | WallLayer | TopWallLayer );
            if (hit.collider == null)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, origin + dir * remaining);
                break;
            }
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
            if (((1 << hit.collider.gameObject.layer) & CellLayer) != 0)
                break;
            if (((1 << hit.collider.gameObject.layer) & TopWallLayer) != 0)
                break;
            dir = Vector2.Reflect(dir, hit.normal);
            origin = hit.point + dir * 0.001f;
        }
        currentCell.layer = oldLayer;
    }

 public   IEnumerator ShootCellAlongRay()
    {
        isShooting = true;
        lineRenderer.enabled = false;
        Vector2 origin = shootPoint.position;
        Vector2 dir = shootPoint.up;
        int oldLayer = currentCell.layer;
        currentCell.layer = LayerMask.NameToLayer("Ignore Raycast");
        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(origin);
        int maxBounces =5;
        float remaining = 50f;
        Cell hitCell = null;
        bool hitTopWall = false;
        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, remaining, CellLayer | WallLayer | TopWallLayer);
            if (hit.collider == null)
            {
                pathPoints.Add(origin + dir * remaining);
                break;
            }
            if (((1 << hit.collider.gameObject.layer) & CellLayer) != 0)
            {
                hitCell = hit.collider.GetComponent<Cell>();
                pathPoints.Add(hit.point - (Vector2)(dir * 0.05f));
                break;
            }
            if (((1 << hit.collider.gameObject.layer) & TopWallLayer) != 0)
            {
                hitTopWall = true;
                pathPoints.Add(hit.point - (Vector2)(dir * 0.05f));
                break;
            }
            dir = Vector2.Reflect(dir, hit.normal);
            origin = hit.point + dir * 0.001f;
            pathPoints.Add(origin);
        }
        for (int i = 1; i < pathPoints.Count; i++)
        {
            while (Vector3.Distance(currentCell.transform.position, pathPoints[i]) > 0.05f)
            {
                currentCell.transform.position = Vector3.MoveTowards(
                    currentCell.transform.position,
                    pathPoints[i],
                    shootSpeed * Time.deltaTime
                );
                yield return null;
            }
        }
        currentCell.layer = oldLayer;
        if (hitCell != null)
        {
            AttachCell(currentCell, hitCell);
        }
        else if (hitTopWall)
        {
            Vector3 p = pathPoints[pathPoints.Count - 1];
            float radius = GridManager.instance.cellSpacing * 0.5f;
            p.y -= radius;
            currentCell.transform.position = p;
            currentCell.transform.SetParent(null);
            var Currcell = currentCell.GetComponent<Cell>();
            GridManager.instance.allCells.Add(Currcell);
            GridManager.instance.topRowCells.Add(Currcell);
        }
        yield return new WaitForSeconds(0.05f);

        SpawnNewCell();
        lineRenderer.enabled = true;
        isShooting = false;
    }
    public void AttachCell(GameObject shot, Cell hitCell)
    {
        GridManager gm = GridManager.instance;

        float radius = gm.cellSpacing * 0.5f;
        float width = radius * 2f;
        float height = Mathf.Sqrt(3f) * radius;

        Vector2[] offsets = new Vector2[]
        {
    new Vector2(width, 0),
    new Vector2(-width, 0),
    new Vector2(width * 0.5f, height),
    new Vector2(-width * 0.5f, height),
    new Vector2(width * 0.5f, -height),
    new Vector2(-width * 0.5f, -height)
        };

        float minDist = float.MaxValue;
        Vector3 bestPos = Vector3.zero;

        foreach (Vector2 offset in offsets)
        {
            Vector3 candidate = hitCell.transform.position + (Vector3)offset;

            bool occupied = false;

            foreach (Cell c in gm.allCells)
            {
                if (Vector3.Distance(c.transform.position, candidate) < radius)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                float dist = Vector3.Distance(shot.transform.position, candidate);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestPos = candidate;
                }
            }
        }
        shot.transform.position = bestPos;
        Rigidbody2D rb = shot.GetComponent<Rigidbody2D>();
        shot.transform.SetParent(hitCell.transform.parent, true);
        shot.layer = LayerMask.NameToLayer("GridCell");
        Cell shotCell = shot.GetComponent<Cell>();
        gm.allCells.Add(shotCell);
        if (Mathf.Approximately(shot.transform.position.y, hitCell.transform.position.y)
            && gm.topRowCells.Contains(hitCell))
        {
            gm.topRowCells.Add(shotCell);
        }
        if (hitCell.color == shotCell.color)
        {
            List<Cell> cluster = shotCell.GetConnectedSameColorCells();
            if (cluster.Count >= 3)
            {
                foreach (Cell c in cluster)
                {
                    gm.allCells.Remove(c);
                    gm.topRowCells.Remove(c);
                    CellPooler.instance.ReturnCell(c.gameObject);
                }
                AddScore(cluster.Count);
                gm.CheckFloatingCells();
                gm.CheckWinCondition();
            }
        }
    }
    public void AddScore(int amount)
    {
        score += amount;
        UiManager.instance.UpdateScore(score);
    }

}
