using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


public class Shooter : MonoBehaviour
{

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
    public TMP_Text scoreText;
    private int score = 0;
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

    void Update()
    {
        if (isShooting) return;
        AimAtMouse();
        DrawAimingLine();
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ShootCellAlongRay());
        }
    }

    void SpawnNewCell()
    {
        currentCell = CellPooler.instance.GetCell(); //take the cell from the cellpooler instance which has GetCell() method.
        var cell = currentCell.GetComponent<Cell>();//store the cell component of the currentcell with the help of GetComponent<Cell> method.
        cell.setColor((CellColor)Random.Range(0, 3));//randomly assign color to the cell in the colors present in CellColor enum.
        currentCell.transform.position = shootPoint.position;//spawns the cell at the shootpoint location.
        currentCell.SetActive(true);//makes the cell active for visiblity

      // currentCell.layer = LayerMask.NameToLayer("ShooterCell");
        currentCellCollider = currentCell.GetComponent<Collider2D>();//stores the collider component of the cell.
    }

    void AimAtMouse()//helps the shooter to rotate in 180 degrees(-90 to +90)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);//converting screen coordinates into world coordinates
        mousePos.z = 0;//making the z axis =0 because it is 2d game.
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        angle = Mathf.Clamp(angle, -rotateLimit, rotateLimit);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void DrawAimingLine()
    {
        Vector2 origin = shootPoint.position;//takes the location of the shooter as origin
        Vector2 dir = shootPoint.up;//the direction is set to upward
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, origin);//sets the starting point of ray to origin.   
        int maxBounces = 4;//max no of reflections.
        float remaining = 50f;//max distance the ray can travel.
        int oldLayer = currentCell.layer;//stores the current layer into oldlayer
        currentCell.layer = LayerMask.NameToLayer("Ignore Raycast");//sets the cell layer to Ignore Raycast layer
        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin, dir, remaining,
                CellLayer | WallLayer | TopWallLayer
            );

            if (hit.collider == null)//this works if the ray doesnot collide with any object.
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, origin + dir * remaining);
                break;
            }

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);//stop the ray if it collide with cell layer or topwalllayer.
            if (((1 << hit.collider.gameObject.layer) & CellLayer) != 0)
                break;
            if (((1 << hit.collider.gameObject.layer) & TopWallLayer) != 0)
                break;
            dir = Vector2.Reflect(dir, hit.normal);
            origin = hit.point + dir * 0.001f;
        }
        currentCell.layer = oldLayer;
    }

    IEnumerator ShootCellAlongRay()
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

            // Bounce
            dir = Vector2.Reflect(dir, hit.normal);
            origin = hit.point + dir * 0.001f;
            pathPoints.Add(origin);
        }

        // Move along each segment
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

        // Restore layer and handle attachment
        currentCell.layer = oldLayer;
        if (hitCell != null)
        {
            AttachCell(currentCell, hitCell);
        }
        else if (hitTopWall)
        {
            Vector3 p = pathPoints[pathPoints.Count - 1];
            p.y -= 0.4f;
            currentCell.transform.position = p;
            currentCell.transform.SetParent(null);
        }

        yield return new WaitForSeconds(0.05f);

        SpawnNewCell();
        lineRenderer.enabled = true;
        isShooting = false;
    }

    void AttachCell(GameObject shot, Cell hitCell)
    {
        CircleCollider2D hitCol = hitCell.GetComponent<CircleCollider2D>();
        CircleCollider2D shotCol = shot.GetComponent<CircleCollider2D>();

        Vector3 dir = (shot.transform.position - hitCell.transform.position).normalized;

        float snapDistance = hitCol.bounds.extents.x + shotCol.bounds.extents.x + 0.01f;

        Vector3 snapPos = hitCell.transform.position + dir * snapDistance;
        shot.transform.position = snapPos;
        shot.transform.SetParent(hitCell.transform.parent);

     
        shot.layer = LayerMask.NameToLayer("GridCell");


        Cell shotCell = shot.GetComponent<Cell>();
       
        GridManager gm = FindObjectOfType<GridManager>();

       
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


    void AddScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();
    }

}
