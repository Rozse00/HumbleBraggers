using UnityEditorInternal;
using UnityEngine;


public class Slink : MonoBehaviour
{
    public Transform playerCamera;//Players camera transform
    public Transform enemyTransform;//Agents transform
    bool inLineOfSight = false;
    bool enemySpotted = false;
    public float fieldOfView = 30f;

    static string[] enemyState = { "Hide", "Attack", "Following" };
    string currentState = enemyState[2];

    private PlayerHealth playerHealth;
    int distanceToPlayer;
    int attackRange = 5;
    int health = 20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       while (health > 0) 
        {
            checkGaze();
            if (!enemySpotted && distanceToPlayer <= attackRange)//If the enemy isn't spotted and is close enough to attack then set enemy state to attack
            {
                currentState = enemyState[1];
            }
            else if (!enemySpotted && distanceToPlayer > attackRange)//If enemy is not spotted but not close to attack
            {
                currentState = enemyState[2];
            }
            else if (enemySpotted) 
            {
                currentState = enemyState[0];
            }

            switch (currentState)
            {
                case "Hide":
                    Debug.Log("Slink is hiding");
                    //enemy find nearest vent logic
                    //enemy moves to nearest vent logic
                    //play enemy fast slithering animation
                    break;
                case "Attack":
                    Debug.Log("Slink is attacking");
                    playerHealth -= 5;
                    //run attack animation
                    break;
                case "Following":
                    Debug.Log("Slink is following");
                    //enemy locate player logic
                    //enemy slither toward player logic
                    //play enemy slow slithering animation
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
        if (hit.transform.CompareTag("Player")) 
        {
        Debug.Log("Player spots the Slink");
        enemySpotted = true;
        }
    }

}//Class end
