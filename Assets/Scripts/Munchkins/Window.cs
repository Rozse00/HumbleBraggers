using UnityEngine;
using System;

public class Window : MonoBehaviour, IInteractable
{
    /// TODO: 
    /// - Add visual sprites for the window when it is closed and opened
    /// - Add audio cues for the window opening

    #region Variables
    [Header("Setup")]
    [SerializeField] private bool startOpen = true;
    [SerializeField] private Transform appearPoint;
    [SerializeField] private float reopenDelayMin = 15f;
    [SerializeField] private float reopenDelayMax = 25f;
    private float reopenTimer; 

    [Header("Temporary Visual Colors")]
    [SerializeField] private Renderer renderer; 
    [SerializeField] private Color openColor = Color.green;
    [SerializeField] private Color closedColor = Color.red;
    [SerializeField] private Color occupiedColor = Color.yellow;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP
    private static readonly int ColorId = Shader.PropertyToID("_Color");         // Built-in
    private MaterialPropertyBlock block;

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

        /// Delete once sprites are set up
        if (renderer == null)
        {
            renderer = GetComponentInChildren<Renderer>(); 
        }

        UpdateColor(); 
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime; 
        }

        if (IsClosed && reopenTimer > 0f)
        {
            reopenTimer -= Time.deltaTime;
            if (reopenTimer <= 0f) Open();
        }
    }
    #endregion

    #region API 
    public void Open()
    {
        IsOpen = true;
        reopenTimer = 0f;
        UpdateColor();
    }

    // Called from the Player and Ally Scripts
    public void Close()
    {
        if (IsClosed)
        {
            return; 
        }

        IsOpen = false;
        reopenTimer = UnityEngine.Random.Range(reopenDelayMin, reopenDelayMax);
        UpdateColor();
        OnClosed?.Invoke(this); 

    }

    public void SetOccupied(bool occupied){
        IsOccupied = occupied;
        UpdateColor(); 
    }

    public void StartCooldown(float seconds) => CooldownRemaining = seconds;
    #endregion

    #region IInteractable
    public bool CanInteract(GameObject interactor) => IsOpen;
    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) Close();
    }
    #endregion

    /// <summary>
    /// Delete after sprites are setup 
    /// </summary>
    private void UpdateColor()
    {
        if (renderer == null)
        {
            return; 
        }

        Color c = IsClosed ? closedColor : (IsOccupied ? occupiedColor : openColor);

        if (block == null) block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor(BaseColorId, c);
        block.SetColor(ColorId, c);
        renderer.SetPropertyBlock(block); 
    }
}
