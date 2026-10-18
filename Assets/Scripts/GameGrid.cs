using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameGrid : MonoBehaviour
{
    public GameObject sphere;
    [HideInInspector] public List<Vector3> spots;

    float left, right, bottom, top;

    void Start()
    {
        left = bottom = -10f;
        right = top = 10f;

        for(float i = left; i<=right; i+=.5f)
        {
            for (float j = bottom; j <= top; j+= .5f)
            {
                Vector3 vec = new Vector3(i, .5f, j);
                spots.Add(vec);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach(Vector3 spot in spots)
        {
            Gizmos.DrawSphere(spot, .24f);
        }
    }
}
