using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CellColor { Red, Green, Blue,Pink };
public class Cell : MonoBehaviour
{
    public CellColor color;
    private SpriteRenderer sr;
    public float neighborCheckRadius = 0.5f;


    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void setColor(CellColor color)
    {
        this.color = color;

        switch (color)
        {
            case CellColor.Red: sr.color = Color.red; break;
            case CellColor.Green: sr.color = Color.green; break;
            case CellColor.Blue: sr.color = Color.blue; break;
        }
    }
    public List<Cell> GetConnectedSameColorCells()
    {
        List<Cell> connected = new List<Cell>();
        Queue<Cell> toCheck = new Queue<Cell>();
        HashSet<Cell> visited = new HashSet<Cell>();

        toCheck.Enqueue(this);
        visited.Add(this);

        while (toCheck.Count > 0)
        {
            Cell current = toCheck.Dequeue();
            connected.Add(current);

            Collider2D[] nearby = Physics2D.OverlapCircleAll(current.transform.position, neighborCheckRadius);

            foreach (var col in nearby)
            {
                Cell neighbor = col.GetComponent<Cell>();
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                float dist = Vector2.Distance(current.transform.position, neighbor.transform.position);
                if (neighbor.color == color && dist < neighborCheckRadius)
                {
                    visited.Add(neighbor);
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        return connected;
    }

    public List<Cell> GetConnectedCells()
    {
        List<Cell> connected = new List<Cell>();
        Queue<Cell> toCheck = new Queue<Cell>();
        HashSet<Cell> visited = new HashSet<Cell>();

        toCheck.Enqueue(this);
        visited.Add(this);

        while (toCheck.Count > 0)
        {
            Cell current = toCheck.Dequeue();
            connected.Add(current);

            Collider2D[] nearby = Physics2D.OverlapCircleAll(current.transform.position, neighborCheckRadius);

            foreach (var col in nearby)
            {
                Cell neighbor = col.GetComponent<Cell>();
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                float dist = Vector2.Distance(current.transform.position, neighbor.transform.position);

             
                if (dist < neighborCheckRadius)
                {
                    visited.Add(neighbor);
                    toCheck.Enqueue(neighbor);
                }
            }
        }

        return connected;
    }

    public void FallAndDisable()
    {
        transform.SetParent(null);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        StartCoroutine(DisableAfterFall());
    }

    IEnumerator DisableAfterFall()
    {
        yield return new WaitForSeconds(2f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CellPooler.instance.ReturnCell(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (gameObject.layer == LayerMask.NameToLayer("GridCell") &&
        other.gameObject.layer == LayerMask.NameToLayer("LoseBar"))
        {
            Debug.Log("GAME OVER TRIGGERED");

            UiManager.instance.GameOver();
        }
    }


}

