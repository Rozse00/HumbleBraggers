using UnityEngine;
using System;
using System.Collections.Generic; 

public interface IAnomaly 
{
    #region Variables
    // States whether or not the anomaly is active currently 
    bool IsActive { get; }

    Transform InteractionPoint { get; } // Where the helper must go to disable the anomaly, pathfinding location
    #endregion

    public float GetUtility();

    bool TryResolve(GameObject resolver); 
}
