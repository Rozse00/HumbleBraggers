using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patrol_AI : MonoBehaviour
{
    public float sphereRad;
    Color col;

    int trig = 0;

    List<Vector3> positions;

    public Transform[] path1;
    public Transform[] path2;

    private GameGrid gameGrid;

    void Start()
    {
        positions = new List<Vector3>();

        float scl = transform.localScale.x;
        float val = sphereRad / scl;
        GetComponent<SphereCollider>().radius = val;

        col = Color.white;


    }

    void OnDrawGizmos()
    {
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, sphereRad);
    }

    void Update()
    {
        Debug.Log(trig);

        if (trig>0) 
            col = new Color(.7f, .4f, .1f, .25f);
        else 
            col = new Color(.5f, .7f, .2f, .4f);
    }

    void OnTriggerEnter(Collider col)
    {
        trig++;
    }

    void OnTriggerStay(Collider col)
    {
        
    }

    void OnTriggerExit(Collider col)
    {
        trig = Mathf.Max(trig-1, 0);
    }

}
