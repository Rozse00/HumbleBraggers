using UnityEngine;

public class BasicAnomalyTest : MonoBehaviour
{
    public float value = 0f;
    public float probability = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetUtility()
    {
        return (value * probability) - ((1 - probability) * 5);
    }
}
