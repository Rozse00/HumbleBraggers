using UnityEditorInternal;
using UnityEngine;

/*
 * Anneliese Hildoer
 * This script will be attached to the slink
 The slink follows the player's camera

To do before finished:
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
    public float fieldOfView = 30f;

    static string[] enemyState = { "Hide", "Attack", "Following" };
    string currentState = enemyState[2];
    public Animator animator;//Attach enemy's animator component
    public float followSpeed = 4f;
    public float hideSpeed = 7f;//Faster speed when hiding
    private Transform currentWP;

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

            checkGaze();//Checks if the enemy is spotted

            //Check to make sure the agent goes to the hiding waypoints before leaving hiding state
            if (currentState == "Hide" && currentWP != null)
            {

                float distanceToDoor = Vector3.Distance(transform.position, currentWP.position);
                //If the slink hasn't reached the door it will be forced to continue moving until remaining distance is closed
                if (distanceToDoor > 0.5f)
                {
                    //Hide Movement 
                    transform.position = Vector3.MoveTowards(transform.position, currentWP.position, hideSpeed * Time.deltaTime);

                    Vector3 lookDir = (currentWP.position - transform.position).normalized;
                    lookDir.y = 0;
                    if (lookDir != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                    }

                    animator.SetTrigger("TriggerHide");
                    return;//Skip state evaluation until arrival
                }
                else
                {
                    //Reached the door, clear waypoint
                    currentWP = null;
                }
            }
                if (!inLineOfSight && distanceToPlayer <= attackRange)//If the enemy isn't spotted and is close enough to attack then set enemy state to attack
            {
                currentState = enemyState[1];//attack
            }
            else if (!inLineOfSight && distanceToPlayer > attackRange)//If enemy is not spotted but not close to attack
            {
                currentState = enemyState[2];//follow
            }
            else if (inLineOfSight)//If enemy is spotted 
            {
                currentState = enemyState[0];//hide
            }

            switch (currentState)
            {
                case "Hide":
                        Debug.Log("Slink is hiding");

                        //Find closest door waypoint
                        if (currentWP == null)
                        {
                            GameObject[] doorWaypoints = GameObject.FindGameObjectsWithTag("Door");
                            if (doorWaypoints.Length > 0)
                            {
                                float shortestDistance = Mathf.Infinity;
                                foreach (GameObject wp in doorWaypoints)
                                {
                                    float distanceToWaypoint = Vector3.Distance(transform.position, wp.transform.position);
                                    if (distanceToWaypoint < shortestDistance)
                                    {
                                        shortestDistance = distanceToWaypoint;
                                        currentWP = wp.transform;
                                    }
                                }
                            }
                        }

                        //Move toward the target door
                        if (currentWP != null)
                        {
                            transform.position = Vector3.MoveTowards(transform.position, currentWP.position, hideSpeed * Time.deltaTime);

                            Vector3 lookDir = (currentWP.position - transform.position).normalized;
                            lookDir.y = 0;
                            if (lookDir != Vector3.zero)
                            {
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                            }
                        }

                        animator.SetTrigger("TriggerHide");
                        break;
                case "Attack":
                    Debug.Log("Slink is attacking");
                    //Snake roatates towards player and attacks
                    if (playerCamera != null)
                    {
                        Vector3 lookDir = (playerCamera.position - transform.position).normalized;
                        lookDir.y = 0;
                        if (lookDir != Vector3.zero)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                        }
                    }

                    animator.SetTrigger("TriggerAttack");//play attack animation
                    //playerHealth.TakeDamage(5);//apply damage to player
                    break;
                case "Following":
                    Debug.Log("Slink is following");
                    //locate and slither toward player logic
                    if (playerCamera != null)
                    {
                            Vector3 targetPos = new Vector3(playerCamera.position.x, transform.position.y, playerCamera.position.z);
                            transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

                            Vector3 lookDir = (targetPos - transform.position).normalized;
                            lookDir.y = 0;
                            if (lookDir != Vector3.zero)
                            {
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                            }
                        }
                    animator.SetTrigger("TriggerFollow");//play enemy slow slithering animation
                    break;
            }
        }
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
            inLineOfSight = true;
        }
    }

    public void TakeDamage(int damageAmount) 
    {
        health -= damageAmount;
        Debug.Log("Slink took " + damageAmount + "damage! Current health: " + health);
        if (health <= 0) 
        {
            Debug.Log("Slink dies");
            Destroy(gameObject);
        }
    }
}//Class end
