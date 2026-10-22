using UnityEngine;
using System; 

public class Window : MonoBehaviour
{
    /// TODO: 
    /// - Add visual sprites for the window when it is closed and opened
    /// - Add audio cues for the window opening
   
    #region Variables
    [Header("Setup")]
    [SerializeField] private bool startOpen = true;
    [SerializeField] private Transform appearPoint;

    public Transform AppearPoint => appearPoint != null ? appearPoint : transform; 

    public bool IsOpen { get; private set; }
    public bool IsClosed => !IsOpen; 
    public bool IsOccupied { get; private set; }
    public bool IsUsable => IsOpen && !IsOccupied && CooldownRemaining <= 0f; 

    public float CooldownRemaining { get; private set; }
    
    // Fired whenever a window is closed, checked by Munchkins to see if it was the window they were at
    public event Action<Window> OnClosed;
    #endregion


    #region Monobehavior Methods
    private void Awake()
    {
        IsOpen = startOpen; 
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime; 
        }
    }
    #endregion

    #region API 
    public void Open() => IsOpen = true; 

    // Called from the Player and Ally Scripts
    public void Close()
    {
        if (IsClosed)
        {
            return; 
        }

        IsOpen = false;
        OnClosed?.Invoke(this); 

    }

    public void SetOccupied(bool occupied) => IsOccupied = occupied;

    public void StartCooldown(float seconds) => CooldownRemaining = seconds; 
    #endregion
}
