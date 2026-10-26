using UnityEngine;
using System;
using System.Collections.Generic; 


public class PlayerHealth : MonoBehaviour
{
    #region Variables
    [Header("Health Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startHealth = 100f;

    [Header("Regen Stats")]
    [SerializeField] private bool regenEnabled = true;
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private float regenPerSecond = 3f;

    private float timeSinceDamage;
    public float MaxHealth => maxHealth; 
    public float CurrentHealth { get; private set; }
    public float Normalized => maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
    public bool IsDead => CurrentHealth <= 0f;
    public bool IsFull => CurrentHealth >= maxHealth;

    public event Action<float, float> OnHealthChanged; // Event for future sound and visual effects
    public event Action<float> OnDamaged;
    public event Action OnDied;
    #endregion

    #region Monobehavior Methods
    private void Awake()
    {
        CurrentHealth = Mathf.Clamp(startHealth, 0f, maxHealth); 
    }

    private void Update()
    {
        if (IsDead)
        {
            return; 
        }

        timeSinceDamage += Time.deltaTime;

        float healthThisFrame = 0f; 

        if (regenEnabled && timeSinceDamage >= regenDelay)
        {
            healthThisFrame += regenPerSecond * Time.deltaTime; 
        }
    }
    #endregion

    #region API
    public float TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return 0f; 
        }

        float dealt = Mathf.Min(amount, CurrentHealth);
        CurrentHealth -= dealt;
        timeSinceDamage = 0f;

        OnDamaged?.Invoke(dealt);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth); 

        if (CurrentHealth <= 0f)
        {
            OnDied?.Invoke(); 
        }

        return dealt; 
    }

    public void ApplyDamage(float amount) => TakeDamage(amount);
    #endregion
}
