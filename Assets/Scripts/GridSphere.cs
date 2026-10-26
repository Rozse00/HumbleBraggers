using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GridSphere : MonoBehaviour
{
    public Point point;

    HashSet<GameObject> objects; //objects currently at point

    [HideInInspector] public Color gizColor { get; set; }

    void Start()
    {
        objects = new HashSet<GameObject>();

        gizColor = Color.gray;
    }

    public void SetPoint(int x, int z)
    {
        point = new Point(x,z);
    }

    public HashSet<GameObject> GetObjects()
    {
        return objects;
    }

    public void SetColor(Color color){gizColor = color;}

    void OnTriggerEnter(Collider col)
    {
        objects.Add(col.gameObject);

        //Debug.Log($"Sphere {point} added "+col.gameObject.name);
    }

    void OnTriggerExit(Collider col)
    {
        objects.Remove(col.gameObject);
    }
}