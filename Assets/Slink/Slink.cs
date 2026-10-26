using UnityEngine;

/*
 * Anneliese Hildoer
 * This script will be attached to the slink
 The slink follows the player's camera

To do before finished:
 ~In main scene attach player transform in inspector
 ~In main scene attack enemies animator component
 ~In main scene make waypoints in doorways and tag them "Door"
 ~Check if reference in the attack case to the method that applies damage to the player is accurate to the player script (line 142)
 */
public class Slink : MonoBehaviour
{
    public Transform playerCamera;//Attach players camera transform
    bool inLineOfSight = false;
    public float fieldOfView = 30f;

    static string[] enemyState = { "Hide", "Attack", "Following" };
    string currentState = enemyState[2];
    string previousState = "";

    public Animator animator;//Attach enemy's animator component
    public float followSpeed = 4f;
    public float hideSpeed = 7f;//Faster speed when hiding
    private Transform currentWP;

    public PlayerHealth playerHealth;//If the players health is a component on the player object
    float distanceToPlayer;
    float attackRange = 5f;
    int health = 20;
    int attackDamage = 5;

    // Update is called once per frame
    void Update()
    {
       if (health > 0) 
        {
            //Calculate distance from slink to player each frame
            if (playerCamera != null)
            {
                distanceToPlayer = Vector3.Distance(transform.position, playerCamera.position);
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

                    SetAnimatorState("Hide");
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

                    SetAnimatorState("Hide");
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

                    SetAnimatorState("Attack"); //play attack animation
                    playerHealth.TakeDamage(attackDamage);//apply damage to player
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
                    SetAnimatorState("Following"); //play enemy slow slithering animation
                    break;
            }
        }
    }

    //Generates a ray starting at the player and checks if it contacts the slink
    private void checkGaze() 
    {
        if (playerCamera == null) return;

        //Calculate the direction from the player camera to the Slink
        Vector3 directionToSlink = (transform.position - playerCamera.position).normalized;
        //Cast ray starting at player
        Ray ray = new Ray(playerCamera.position, directionToSlink);
        RaycastHit hit;

        //Calculate exact distance to avoid raycast missing due to range
        float currentDistance = Vector3.Distance(playerCamera.position, transform.position);

        //Cast a ray from the player's camera toward the Slink
        if (Physics.Raycast(ray, out hit, currentDistance + 1f))
        {
            //Check if the ray hit the Slink before hitting a wall
            if (hit.transform == transform)
            {
                Debug.Log("Player spots the Slink - running to hide!");
                inLineOfSight = true;
                return;
            }
        }

        //If the ray hit a wall/obstacle instead of the Slink, or is out of sight, it's not in line of sight
        inLineOfSight = false;
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

    //Only trigger the animation parameter when changing states to avoid spamming/freezing triggers
    private void SetAnimatorState(string newState)
    {
        if (newState != previousState && animator != null)
        {
            animator.ResetTrigger("TriggerHide");
            animator.ResetTrigger("TriggerAttack");
            animator.ResetTrigger("TriggerFollow");

            if (newState == "Hide") animator.SetTrigger("TriggerHide");
            else if (newState == "Attack") animator.SetTrigger("TriggerAttack");
            else if (newState == "Following") animator.SetTrigger("TriggerFollow");

            previousState = newState;
        }
    }

}//Class end
