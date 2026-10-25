using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ExorcistAgent : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<BasicAnomalyTest> anom = GetAnomalies();
        string result = string.Join(", ", anom.Select(x => x.ToString()));
        Debug.Log($"List contents: {result}\n");
        BasicAnomalyTest target = ChooseTarget(anom);
        Debug.Log(target.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public List<BasicAnomalyTest> GetAnomalies()
    {
        BasicAnomalyTest[] foundAnomalies = FindObjectsByType<BasicAnomalyTest>(FindObjectsSortMode.None);
        return new List<BasicAnomalyTest>(foundAnomalies);
    }
    public BasicAnomalyTest ChooseTarget(List<BasicAnomalyTest> anomalies)
    {
        float bestExpectedUtility = -999f;
        BasicAnomalyTest bestAnomaly = null;
        for(int i = 0; i < anomalies.Count; i++)
        {
            float currentUtility = anomalies[i].GetUtility();
            if(currentUtility > bestExpectedUtility)
            {
                bestExpectedUtility = currentUtility;
                bestAnomaly = anomalies[i];
            }
        }
        return bestAnomaly;
    }
}
