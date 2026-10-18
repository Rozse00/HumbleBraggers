using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patrol_AI : MonoBehaviour
{
    //radius of detection Sphere
    public float sphereRad;
    Color col;

    int trig = 0;
    
    //might store positions of objects to avoid, i.e. a wall to move away from when moving around;
    List<Vector3> positions;
    
    //will store waypoint lists for patrolling
    public Transform[] path1;
    public Transform[] path2;

    GridSearch gs; 
    public GameObject lookForThis;

    void Start()
    {
        positions = new List<Vector3>();

        float scl = transform.localScale.x;
        float val = sphereRad / scl;
        GetComponent<SphereCollider>().radius = val;

        gs = GetComponent<GridSearch>();

        col = Color.white;
    }

    //List<Transform>
    public void GetPath()
    {
        List<Transform> result = gs.Search(gameObject.transform.position, lookForThis);

        if (result == null || result.Count == 0)
            Debug.LogWarning($"Search issue | {gameObject.name} looking for {lookForThis.name} ");
        else
            Debug.Log($"Found {lookForThis.name}! ");

    }

    void OnDrawGizmos()
    {
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, sphereRad);
    }

    void Update()
    {
        if (trig>0) 
            col = new Color(.7f, .4f, .1f, .25f);
        else 
            col = new Color(.5f, .7f, .2f, .4f);
    }

    void OnTriggerEnter(Collider col)
    {
        trig++;
    }

    void OnTriggerExit(Collider col)
    {
        trig = Mathf.Max(trig-1, 0);
    }

}
