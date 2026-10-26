using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameGrid : MonoBehaviour
{
    public GameObject sphere;
    [HideInInspector] public List<List<GameObject>> spheres; //stores all the spheres used in the grid

    public bool showGizmos;

    public enum Mode
    {
        Bounds, CornerPoints
    }
    public Mode mode;

    public float offset = .75f; //dist between grid points

    [Header("X-bounds")]
    public float left;
    public float right;
    [Header("Z-bounds")]
    public float bottom;
    public float top;

    [Header("Corner Points")]
    public Transform bLeft;
    public Transform tRight;

    

    void Awake()
    {
        if(mode == Mode.CornerPoints)
        {
            left = bLeft.position.x; bottom = bLeft.position.z;
            right = tRight.position.x; top = tRight.position.z;
        }

        spheres = new List<List<GameObject>>();

        for (float i = left; i<=right; i+=offset)
        {
            List<GameObject> line = new List<GameObject>();

            for (float j = bottom; j <= top; j+= offset)
            {
                Vector3 vec = new Vector3(i, .5f, j);
                GameObject obj = Instantiate(sphere, this.transform);
                obj.transform.position = vec;
                line.Add(obj);
            }

            spheres.Add(line);
        }

        for (int i = 0; i < spheres.Count; i++)
        {
            for (int j = 0; j < spheres[i].Count; j++)
            {
                GridSphere gs = spheres[i][j].GetComponent<GridSphere>();
                gs.SetPoint(i, j);
            }
        }

        Debug.Log($"{spheres.Count} * {spheres[0].Count} = {spheres.Count * spheres[0].Count}");

        //Physics.SyncTransforms();
        ResetColors();
    }

    public GameObject Get(Point p)
    {
        return spheres[p.x][p.z];
    }
    public float GetOffset() {  return offset; }

    public void ResetColors()
    {
        Debug.Log("Reset Colors.");
        foreach(List<GameObject> line in spheres)
        {
            for (int i = 0; i < line.Count; i++)
            {
                line[i].GetComponent<GridSphere>().gizColor = Color.gray;
            }
        }
    }

    public void OnDrawGizmos()
    {
        if(!showGizmos) return;

        if (spheres == null || spheres.Count == 0)
        {
            return;
        }

        for (int i = 0; i < spheres.Count; i++)
        {
            for (int j = 0; j < spheres[0].Count; j++)
            {
                GameObject sp = spheres[i][j];
                Gizmos.color = sp.GetComponent<GridSphere>().gizColor;
                Gizmos.DrawSphere(sp.transform.position, offset / 2);
            }
        }
    }
}
