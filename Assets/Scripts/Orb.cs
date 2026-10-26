using System; 
using UnityEngine;

public class Orb : MonoBehaviour
{
    #region Variables

    // Singleton so other classes can reference the orb's methods if necessary
    public static Orb Instance { get; private set; }

    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float startEnergy = 100f;

    public float MaxEnergy => maxEnergy; 
    public float Energy { get; private set; }
    public bool isFull => Energy >= maxEnergy;
    public bool isEmpty => Energy <= 0f;

    public event Action<float, float> OnEnergyChange; // Event to allow other methods to easily fire upon changes to Orb Energy
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; 
        }

        Instance = this; 
        Energy = Mathf.Clamp(startEnergy, 0f, maxEnergy);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null; 
        }
    }

    private void OnEnable()
    {
        OnEnergyChange += HandleEnergyChange; 
    }

    private void OnDisable()
    {
        OnEnergyChange -= HandleEnergyChange; 
    }

    /// <summary>
    /// Method to take energy from the orb. Returns the amount taken. 
    /// </summary>
    public float Drain(float amount)
    {
        if (amount <= 0f)
        {
            return 0f;
        }

        float taken = Mathf.Min(amount, Energy);

        if (taken <= 0f)
        {
            return 0f;
        }

        Energy -= taken;
        OnEnergyChange?.Invoke(Energy, -taken);
        return taken; 
    }

    public void DrainEnergy(float amount)
    {
        Drain(amount);
    }

    /// <summary>
    /// Method to add energy to the orb. Returns the amount added. 
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public float Recharge(float amount)
    {
        if (amount <= 0f)
        {
            return 0f; 
        }

        float charged = Mathf.Min(amount, maxEnergy - Energy); 

        if (charged <= 0f)
        {
            return 0f; 
        }

        Energy += charged;
        OnEnergyChange?.Invoke(Energy, charged);
        return charged; 
    }

    /// <summary>
    /// Placeholder method to handle visual changes, play sound effects, and other cases depending on how the orb's energy is being changed. 
    /// Also handles the orb's state machine. 
    /// </summary>
    /// <param name="newEnergy"></param>
    /// <param name="delta"></param>
    private void HandleEnergyChange(float newEnergy, float delta)
    {
        
    }
}
