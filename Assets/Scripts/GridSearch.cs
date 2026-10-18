using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GridSearch : MonoBehaviour
{
    static GameGrid gameGrid;
    static List<List<GameObject>> spheresRef;

    public void Start()
    {
        gameGrid = GameObject.Find("GameGridObj").GetComponent<GameGrid>();
        spheresRef = gameGrid.spheres;
    }

    //public List<Transform> Search(Transform tf, GameObject obj)
    //{
    //    return Search(tf, obj);
    //}

    public List<Transform> Search(Vector3 orig, GameObject searchObj)
    {
        gameGrid.ResetColors();

        Collider temp = GetClosestSphere(orig);
        if (temp == null)
            return null;

        Point sPoint = temp.gameObject.GetComponent<GridSphere>().point;

        HashSet<Point> set = new HashSet<Point>(); set.Add(sPoint);
          Queue<Point> q = new Queue<Point>(); q.Enqueue(sPoint);

        Dictionary<Point, Point> prev = new Dictionary<Point, Point>();

        Point end = null;

        while (q.Count > 0)
        {
            StartCoroutine(Delay(q));

            SpreadOut(q, set,  prev);

            StringBuilder s = new StringBuilder();

            int size = q.Count;
            for (int i = 0; i < size; i++)
            {
                Point next = q.Dequeue();
                GridSphere gSphere = gameGrid.Get(next).GetComponent<GridSphere>();
                gSphere.gizColor = Color.darkBlue;

                string str = "";
                foreach(GameObject obj in gSphere.GetObjects())
                {
                    string add = obj.name;
                    if (str == "")
                        add = ", " + add;
                    str += add;
                }
                s.Append($"[{str}]\n");

                q.Enqueue(next);
            }
        }

        if (end == null)
            return null;

        List<Transform> path = new List<Transform>();
        Point at = end;

        StringBuilder sb = new StringBuilder();

        while (at != null)
        {
            Debug.Log("(I)");
            if (sb.Length == 0)
                sb.Append(at);
            else
                sb.Insert(0, at+ " -> ");

            GameObject go = spheresRef[at.x][at.z];
            go.GetComponent<GridSphere>().gizColor = Color.red;
            path.Add(go.transform);

            at = prev[at];
        }

        Debug.Log("Path:  "+sb.ToString());

        path.Reverse();

        return path;
    }

    IEnumerator Delay(Queue<Point> q)
    {
        yield return new WaitForSeconds(0.5f);
        int size = q.Count;

        for (int i = 0; i < size; i++)
        {
            Point next = q.Dequeue();
            GridSphere gSphere = gameGrid.Get(next).GetComponent<GridSphere>();
            gSphere.gizColor = Color.darkBlue;
            q.Enqueue(next);
        }
    }

    void SpreadOut(Queue<Point> q, HashSet<Point> set, Dictionary<Point, Point> prev)
    {
        int size = q.Count;

        for (int i = 0; i < size; i++)
        {
            Point current = q.Dequeue();

            int X = current.x, Z = current.z;

            if (X > 0)
            {
                Point next = new Point(X - 1, Z);
                if (!set.Contains(next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (X < spheresRef.Count - 1)
            {
                Point next = new Point(X + 1, Z);
                if (!set.Contains(next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (Z > 0)
            {
                Point next = new Point(X, Z - 1);
                if (!set.Contains(next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (Z < spheresRef[0].Count - 1)
            {
                Point next = new Point(X, Z + 1);
                if (!set.Contains(next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
        }
    }

    Collider GetClosestSphere(Vector3 orig)
    {
        Collider[] hitColliders = Physics.OverlapSphere(orig, .5f);

        Collider startCol = null;
        float dist = 200f;

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject.GetComponent<GridSphere>() == null)
                continue;

            float d = Vector3.Distance(col.gameObject.transform.position, orig);

            if (d < dist)
            {
                dist = d;
                startCol = col;
            }
        }

        if (startCol == null)
        {
            Debug.LogWarning("Issue with sphere detection");
            return null;
        }

        return startCol;
    }
}
