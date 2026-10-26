using UnityEngine;
using System;
using System.Collections.Generic; 

public interface IAnomaly 
{
    #region Variables
    // Name of the Anomaly
    string name { get; set; }
    // States whether or not the anomaly is active currently 
    bool IsActive { get; }
    // the GameObject the anomaly is contained in
    GameObject gameObject { get; }

    Transform InteractionPoint { get; } // Where the helper must go to disable the anomaly, pathfinding location
    #endregion

    public float GetUtility();

    // Called by ally to resolve what the anamoly is. 
    // The anamoly must interpret what its resolution is
    bool TryResolve(GameObject resolver); 
}

// Shared anomaly utilty calculation formula 
public static class AnomalyUtilityCalculator
{
    public static float ExpectedUtility(float value, float successProb, float failurePenalty = 5)
    {
        float prob = Mathf.Clamp01(successProb);
        return value * prob - (1f - prob) * failurePenalty; 
    }
}


/// <summary>
/// Acts as a registry for anamolies. The anomalies fire active and resolved events as they work which the ally subscribes to and listens to. The ally is aware
/// of all anomalies on the map as they are registered and stored in the list. 
/// </summary>
public static class AnomalyEvents
{
    private static readonly List<IAnomaly> active = new List<IAnomaly>();

    public  static IReadOnlyList<IAnomaly> Active => active;

    public static event Action<IAnomaly> Activated;
    public static event Action<IAnomaly> Resolved;

    // Called by an anomaly after they become active
    // The anomaly is added to the active list, which the ally reads 
    public static void RaiseActivated(IAnomaly anomaly)
    {
        if (anomaly == null || active.Contains(anomaly))
        {
            return; 
        }

        active.Add(anomaly);
        Activated?.Invoke(anomaly); 
    }

    // Called by anomaly whenever it's caught or resolved, therefore no longer active
    // The Anomaly is removed from teh active list and a resolved event is fired so the ally can switch to another target if there are any
    public static void RaiseResolved(IAnomaly anomaly)
    {
        if (anomaly == null || active.Remove(anomaly))
        {
            return;
        }
        Resolved?.Invoke(anomaly);
    }

    //Resets the static variables whenever a scene is loaded, domain reset, etc. as they persist 
    //RuntimeInitializeOnLoad ensures it is always called whenver play is pressed in the editor 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        active.Clear();
        Activated = null;
        Resolved = null; 
    }

}