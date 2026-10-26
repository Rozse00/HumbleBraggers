using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ExorcistAgent : MonoBehaviour
{
    // Frequency at which the Exorcist will recalculate the utility of certain events
    [SerializeField] private float evaluationInterval = 1f;
    [SerializeField] private float tempDispelTimer = 2f; 
    private float  intervalTimer;
    private float  dispelTimer; 
    

    public IAnomaly Target { get; private set;  }
    private GridSearch gs; // Socket for future travel implementation 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //List<IAnomaly> anom = GetAnomalies();
        //string result = string.Join(", ", anom.Select(x => x.ToString()));
        //Debug.Log($"List contents: {result}\n");
        Reevaluate();
        //Debug.Log(target.ToString());
    }

    // Subscribe to the anomaly events upon creation and unsubscribe upon defeat/disable
    void OnEnable()
    {
        AnomalyEvents.Activated += HandleAnomalyChanged;
        AnomalyEvents.Resolved += HandleAnomalyChanged;
        Reevaluate(); 
    }

    void OnDisable()
    {
        AnomalyEvents.Activated -= HandleAnomalyChanged;
        AnomalyEvents.Resolved -= HandleAnomalyChanged;
    }

    // Update is called once per frame
    void Update()
    {
         intervalTimer -= Time.deltaTime;
        if ( intervalTimer <= 0f)
        {
            Reevaluate(); 
        }

        //Temporary Solution for dispelling entities
        TickDispel(); 

        // CODE FOR PATHFINDING TOWARDS TARGET HERE
        // The ExorcistAgent is moving towards the anaomaly's InteractionPoint (a transform component) 
    }

    //public List<IAnomaly> GetAnomalies()
    //{
    //    IAnomaly[] foundAnomalies = FindObjectsByType<IAnomaly>(FindObjectsSortMode.None);
    //    return new List<IAnomaly>(foundAnomalies);
    //}

    //public BasicAnomalyTest ChooseTarget(List<BasicAnomalyTest> anomalies)
    //{
    //    float bestExpectedUtility = -999f;
    //    BasicAnomalyTest bestAnomaly = null;
    //    for(int i = 0; i < anomalies.Count; i++)
    //    {
    //        float currentUtility = anomalies[i].GetUtility();
    //        if(currentUtility > bestExpectedUtility)
    //        {
    //            bestExpectedUtility = currentUtility;
    //            bestAnomaly = anomalies[i];
    //        }
    //    }
    //    return bestAnomaly;
    //}

    /// <summary>
    /// Methods for IAnomaly 
    /// 
    /// HandleAnomalyChanged is fired by the anomalies whenever they trigger the Activated event
    /// 
    /// Reevaluate is the Reevaluate is the method for determining which anomaly the Exorcist should prioritize when active
    /// 
    /// ChooseTarget() is a rewrite of the BasicAnomalyTest ChooseTarget(), which selects the anomaly with the higher utility and returns it for the agent to prioritize
    /// </summary>
    #region IAnomaly 
    private void HandleAnomalyChanged(IAnomaly anomaly) => Reevaluate();

    private void TickDispel()
    {
        if (Target == null)
        {
            return; 
        }

        dispelTimer -= Time.deltaTime;
        if (dispelTimer > 0f) return;

        IAnomaly dispelling = Target;
        dispelTimer = tempDispelTimer; 

        if (dispelling.TryResolve(gameObject))
        {
            Debug.Log($"[Exorcist] Dispelled {dispelling.name}");
        }
    }

    private void Reevaluate()
    {
         intervalTimer = evaluationInterval;

        IAnomaly previousTarget = Target;
        Target = ChooseTarget();

       /// gs.Search(transform.position, Target.gameObject);

        if (previousTarget != Target)
        {
            dispelTimer = tempDispelTimer;   
            string targetName = Target != null ? Target.name : "None"; 
            Debug.Log($"[Exorcist] Target -> " + targetName);
        }
    }

    public IAnomaly ChooseTarget()
    {
        IAnomaly best = null;
        float bestUtility = -999f;

        // Parse all active anomalies in the AnomalyEvents list
        foreach (IAnomaly anomaly in AnomalyEvents.Active)
        {
            if (!anomaly.IsActive) continue;

            float utility = anomaly.GetUtility();
            if (utility > bestUtility)
            {
                bestUtility = utility;
                best = anomaly;
            }
        }
        
        return best; 
    }
    #endregion 
}
