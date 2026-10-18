using UnityEditorInternal;
using UnityEngine;

/*This script will be attached to the slink
 The slink follows the player's camera assuming this is 1st person

To do before finished:
 ~Add snake character
 ~Add snake animations for following, hiding and attacking
 ~In main scene attach player transform in inspector
 ~In main scene attach agent transform in inspector
 ~In main scene make waypoints in doorways and tag them "Door"
 ~Check if reference in the attack case to the method that applies damage to the player is accurate to the player script
 */
public class Slink : MonoBehaviour
{
    public Transform playerCamera;//Attach players camera transform
    public Transform enemyTransform;//Attach agents transform
    bool inLineOfSight = false;
    bool enemySpotted = false;
    public float fieldOfView = 30f;

    static string[] enemyState = { "Hide", "Attack", "Following" };
    string currentState = enemyState[2];
    public Animator animator;//Attach enemy's animator component
    public UnityEngine.AI.NavMeshAgent navAgent;//Attach navmesh

    //public PlayerHealth playerHealth;//If the players health is a component on the player object
    float distanceToPlayer;
    float attackRange = 5f;
    int health = 20;

    // Update is called once per frame
    void Update()
    {
       if (health > 0) 
        {
            //Calculate distance from slink to player each frame
            if (playerCamera != null)
            {
                distanceToPlayer = Vector3.Distance(enemyTransform.position, playerCamera.position);
            }

            checkGaze();

            //Check to make sure the agent goes to the hiding waypoints before leaving hiding state
            if (currentState == "Hide" && navAgent != null)
            {
                //Check if the agent is still moving to the door
                if (!navAgent.pathPending && navAgent.remainingDistance > navAgent.stoppingDistance)
                {
                    //Exit Update early so the program doesn't reevaluate and change states prematurely
                    return;
                }
            }

            if (!enemySpotted && distanceToPlayer <= attackRange)//If the enemy isn't spotted and is close enough to attack then set enemy state to attack
            {
                currentState = enemyState[1];//attack
            }
            else if (!enemySpotted && distanceToPlayer > attackRange)//If enemy is not spotted but not close to attack
            {
                currentState = enemyState[2];//follow
            }
            else if (enemySpotted)//If enemy is spotted 
            {
                currentState = enemyState[0];//hide
            }

            switch (currentState)
            {
                case "Hide":
                    Debug.Log("Slink is hiding");//play enemy fast slithering animation
                    navAgent.isStopped = false;//Ensures movement is enabled

                    //Locate nearest door waypoint and travel there then start following again
                    GameObject[] ventWaypoints = GameObject.FindGameObjectsWithTag("Door");
                    Transform nearestDoor = null;
                    float shortestDistance = Mathf.Infinity;//Shortest distance starts at longest
                    foreach (GameObject wp in ventWaypoints) //Loop through all door waypoints to find shortest distance
                    {
                        float distanceToWP = Vector3.Distance(transform.position, wp.transform.position);
                        if (distanceToWP < shortestDistance) 
                        {
                            shortestDistance = distanceToWP;
                            nearestDoor = wp.transform;
                        }
                    }
                    navAgent.SetDestination(nearestDoor.position);//Move agent toward the door
                    animator.SetTrigger("TriggerHide");//Play fast sneaking animation/hiding animation
                    break;
                case "Attack":
                    Debug.Log("Slink is attacking");
                    animator.SetTrigger("TriggerAttack");//play attack animation
                    //playerHealth.TakeDamage(5);//apply damage to player
                    break;
                case "Following":
                    Debug.Log("Slink is following");
                    //locate and slither toward player logic
                    if (navAgent != null && playerCamera != null)
                    {
                        navAgent.isStopped = false;//Ensures movement is enabled
                        navAgent.SetDestination(playerCamera.position);
                        animator.SetTrigger("TriggerFollow");//play enemy slow slithering animation
                        Debug.Log("Slink is following");
                    }
                    break;
            }
        }//while alive
    }

    private void checkGaze() 
    {
        //Cast ray
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        //If the ray hits a slink, change enemhySpotted to true
        if (Physics.Raycast(ray, out hit)) 
        {
        Debug.Log("Player spots the Slink");
        enemySpotted = true;
        }
    }
}//Class end
