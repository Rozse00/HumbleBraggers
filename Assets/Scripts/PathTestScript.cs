using UnityEngine;

public class PathTestScript : MonoBehaviour
{
    public Patrol_AI patrol;
    public MessageFlash msg;

    public void OnSprint()
    {
        if(patrol != null)
        {
            patrol.GetPath();
            msg.Flash();
        }
    }
}
