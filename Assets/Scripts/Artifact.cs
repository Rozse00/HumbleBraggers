using UnityEngine;

public class Artifact : MonoBehaviour
{
    #region Variables
    [SerializeField] private Orb orb;
    [SerializeField] private float rechargeAmount = 10f;
    [SerializeField] private bool consumeOnPickup = true; 
    #endregion

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return; 
        }

        // If the orb is full don't apply any charge and preserve the artifact 
        if (orb.Recharge(rechargeAmount) <= 0f)
        {
            return; 
        }

        if (consumeOnPickup)
        {
            gameObject.SetActive(false); 
        }
    }
}
