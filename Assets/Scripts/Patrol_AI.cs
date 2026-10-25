using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Patrol_AI : MonoBehaviour
{
    //radius of sight/detection Sphere
    public float sphereRad;
    GameObject sightObj;
    Color col;

    int trig = 0;

    //might store positions of objects in environment to avoid; currently unused
    //List<Vector3> positions;

    public Transform returnPoint;
    //will store waypoint lists for patrolling
    public List<Transform> route1;
    public List<Transform> route2;
    
    int routeCount = 2;
    int routeIndex = 0;
    List<Transform> currentRoute;
    int waypointIndex = 0;

    GridSearch gs; //GridSearch ref
    List<Transform> currentLine;
    int lineIndex = 0;

    public bool canMove;
    public float moveSpeed;

    bool fixedUpdateError = false;

    float startTime;

    void Start()
    {
        startTime = Time.time;
        //positions = new List<Vector3>();

        float scl = transform.localScale.x;
        float val = sphereRad / scl;
        sightObj = transform.GetChild(0).gameObject;
        sightObj.GetComponent<SphereCollider>().radius = val;

        gs = GetComponent<GridSearch>();

        col = Color.white;

        route1.Add(returnPoint);
        route2.Add(returnPoint);

        Initialize();

        Debug.Log($"{routeIndex} | {waypointIndex} | {lineIndex}"
            + $"\n{currentRoute} | {currentLine}");
    }

    void Initialize()
    {
        currentRoute = route1;
        
        GetCurrentWaypoint();
    }

    public bool GetNextWaypoint()
    {
        waypointIndex++;
        if (waypointIndex == currentRoute.Count)
            return false;

        GetCurrentWaypoint();

        return true;
    }

    public void GetCurrentWaypoint()
    {
        Transform next = currentRoute[waypointIndex];
        currentLine = gs.Search(transform.position, next.gameObject);

        lineIndex = 0;
    }

    public void GetNextRoute()
    {
        if (routeIndex == 0)
            routeIndex = 1;
        else
            routeIndex = 0;

        if (routeIndex == 0)
            currentRoute = route1;
        else
            currentRoute = route2;

        waypointIndex = 0;
    }

    void FixedUpdate()
    {
        if (fixedUpdateError) return;

        Debug.Log("Fixed Update");

        if (!canMove)
            return;

        if (currentLine == null)
        {
            Debug.LogWarning("Issue with currentLine | index = " + lineIndex);
            fixedUpdateError = true;
            return;
        }

        Transform next = currentLine[lineIndex];

        Vector3 groundPoint = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targPoint = new Vector3(next.position.x, 0, next.position.z);

        float dist = Vector3.Distance(groundPoint, targPoint);

        if (dist < .25f)
        {
            lineIndex++;
            if (lineIndex == currentLine.Count)
            {
                if (!GetNextWaypoint())
                {
                    Debug.Log("Finished Route");
                    GetNextRoute();
                    GetCurrentWaypoint();
                }
            }
        }

        next = currentLine[lineIndex];
        transform.LookAt(next.position);
        transform.Translate(transform.forward * Time.fixedDeltaTime * moveSpeed);

        Debug.DrawRay(transform.position, transform.forward, Color.blue);
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

    void OnDrawGizmos()
    {
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, sphereRad);

        if (currentRoute == null) return;

        Gizmos.color = new Color(.75f, .75f, .9f, .25f);
        foreach (Transform waypoint in currentRoute)
        {
            Gizmos.DrawSphere(waypoint.position, .2f);
        }
    }
}
