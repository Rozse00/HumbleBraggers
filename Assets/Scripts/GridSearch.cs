using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//This searches through the grid for any other object on the
//grid; Attach this to any gameObject that will need pathing
public class GridSearch : MonoBehaviour
{
    GameGrid gameGrid;
    List<List<GameObject>> spheresRef;

    //Put the layers for any obstacles/walls here
    public LayerMask mask;

    public void Start()
    {
        gameGrid = GameObject.Find("GameGridObj").GetComponent<GameGrid>();
        spheresRef = gameGrid.spheres;
    }

    public List<Transform> Search(Vector3 orig, GameObject searchObj)
    {
        gameGrid.ResetColors();

        Collider temp = GetClosestSphere(orig);
        if (temp == null)
            return null;

        Point sPoint = temp.gameObject.GetComponent<GridSphere>().point;
        Debug.Log("Started: " + sPoint);

        HashSet<Point> set = new HashSet<Point>(); set.Add(sPoint);
          Queue<Point> q = new Queue<Point>(); q.Enqueue(sPoint);

        Dictionary<Point, Point> prev = new Dictionary<Point, Point>();

        Point end = null;

        while (q.Count > 0)
        {
            SpreadOut(q, set, prev);

            StringBuilder s = new StringBuilder();

            int size = q.Count;
            for (int i = 0; i < size; i++)
            {
                Point next = q.Dequeue();
                GridSphere gSphere = gameGrid.Get(next).GetComponent<GridSphere>();
                gSphere.gizColor = Color.cyan;

                string str = "";
                foreach(GameObject obj in gSphere.GetObjects())
                {
                    string add = obj.name;
                    if (str == "")
                        add = ", " + add;
                    str += add;
                }
                s.Append($"Point {next}: [{str}]\n");

                if( end==null && gSphere.GetObjects().Contains(searchObj))
                {
                    Debug.Log("FOUND IT");
                    end = next;
                    q.Clear();
                    break;
                }

                q.Enqueue(next);
            }
            //shows the objects hit in the last search
            Debug.Log(s.ToString());
        }

        List<Transform> path = new List<Transform>();
        Point at = end;

        StringBuilder sb = new StringBuilder();
        
        //goes backward through prev to get critical path
        while (prev.ContainsKey(at) && prev[at] != null)
        {
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

    //Spreads the search out one layer
    void SpreadOut(Queue<Point> q, HashSet<Point> set, Dictionary<Point, Point> prev)
    {
        int size = q.Count;

        for (int i = 0; i < size; i++)
        {
            Point current = q.Dequeue();

            int X = current.x, Z = current.z;

            //checks up-down-left-right for available spots
            if (X > 0)
            {
                Point next = new Point(X - 1, Z);
                if (!set.Contains(next) && CanMove(current, next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (X < spheresRef.Count - 1)
            {
                Point next = new Point(X + 1, Z);
                if (!set.Contains(next) && CanMove(current, next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (Z > 0)
            {
                Point next = new Point(X, Z - 1);
                if (!set.Contains(next) && CanMove(current, next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
            if (Z < spheresRef[0].Count - 1)
            {
                Point next = new Point(X, Z + 1);
                if (!set.Contains(next) && CanMove(current, next))
                {
                    prev[next] = current;

                    q.Enqueue(next);
                    set.Add(next);
                }
            }
        }
    }

    //Checks if point b can be traveled into from point a
    bool CanMove(Point a, Point b)
    {
        Transform A = gameGrid.Get(a).transform;
        Transform B = gameGrid.Get(b).transform;

        Vector3 dir = B.position - A.position;

        RaycastHit hit;

        bool sphereHit = Physics.SphereCast(A.position, .2f, dir, out hit, gameGrid.GetOffset(), mask);

        return !sphereHit;
    }

    //Get nearest grid unit
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
