using UnityEngine;
using System;
using System.Collections.Generic;


public class Munchkin : MonoBehaviour
{

    public struct AppearanceStats
    {
        public float timeInWindow; // Time Munchkin was in window before being caught
        public float energySiphoned; // Track total siphoned energy before being caught
    }

    #region Variables 
    public enum State { Cooldown, Ready, Active, Siphoning, Caught } // Enums for State Machine

    [Header("References")]
    [SerializeField] private Window[] windows;
    [SerializeField] private Transform player;
    [SerializeField] private Orb orb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearCue;

    [Header("Player Sensor")]
    [SerializeField] private float sensorInterval = 0.5f; // Time inbetween checks for the player location 
    [SerializeField] private float sensorOffset = 2f; // An offset added to the player's X and Z, to create variance in distance calculations. The Munchkin can sometimes pick an inoptimal window

    [Header("Cooldowns & Rates")]
    [SerializeField] private float minCooldown = 10f;
    [SerializeField] private float maxCooldown = 30f;
    [SerializeField] private float siphonDelay = 1.5f;
    [SerializeField] private float windowCooldownAfterUse = 20f; // How long before a Munckin can reappear in the same window 
    [SerializeField] private float noWindowRetryInteveral = 1f; // Interval for reattempting window calculations if a good window wasn't found on the previous attempt
    [SerializeField] private float siphonRatePerSecond = 2f;
    [SerializeField] private float appearDelay = 3f; // Time between choosing a window and the Munchkin appearing in it

    [Header("Utility Weights")]
    [SerializeField] private float playerDistance = 1f; // The further from the player, the better
    [SerializeField] private float jitter = 2f; // Randomness 

    [Header("Runtime")]
    public State CurrentState { get; private set; }
    public readonly List<AppearanceStats> History = new List<AppearanceStats>();

    public event Action<Window> IsSummoned; // Future socket for the WallpaperMan
    public event Action<AppearanceStats> AppearanceEnded;

    private Window targetWindow;
    private Window currentWindow;
    private Window lastWindow;

    private Vector3 sensedPlayerPosition;

    private float sensorTimer;
    private float activityCooldown;
    private float retryTimer;
    private float appearTimer; // Timer before the Munchkin can appear in a window 
    private float siphonTimer;
    private float timeInWindow;
    private float energySiphoned;

    private bool canAppear;
    private bool summoned; // Future socket for the WallpaperMan
    #endregion

    #region Monobehavior Methods 
    private void Start()
    {
        foreach (var window in windows)
        {
            window.OnClosed += HandleWindowClosed;
        }

        HideSprite();
        SampleSensor();
        activityCooldown = ComputeCooldown();
        SetState(State.Cooldown);

    }

    private void OnDestroy()
    {
        IsSummoned -= HandleSummoned;
        if (windows == null) return;
        foreach (var window in windows)
        {
            if (window)
            {
                window.OnClosed -= HandleWindowClosed;
            }
        }
    }

    private void Update()
    {
        UpdatePlayerSensor();

        switch (CurrentState)
        {
            case State.Cooldown:
                TickCooldown();
                break;
            case State.Ready:
                TickReady();
                break;
            case State.Active:
                TickActive();
                break;
            case State.Siphoning:
                TickSiphon();
                break;

        }
    }
    #endregion

    /// <summary>
    ///  Methods that facilitiate the Munchkin's operations for each state
    /// </summary>
    #region State Methods & Transitions
    // Start cooldown counter, transitions into Ready-State once complete
    private void TickCooldown()
    {
        activityCooldown -= Time.deltaTime;
        if (activityCooldown <= 0f && CheckAppearance())
        {
            SetState(State.Ready);
        }
    }

    // Handles target selection, transitions into Active-State once found via AppearInWindow()
    private void TickReady()
    {
        if (targetWindow == null)
        {
            retryTimer -= Time.deltaTime;
            if (retryTimer > 0f) return;

            targetWindow = WindowToAppear();
            if (targetWindow == null)
            {
                retryTimer = noWindowRetryInteveral;
                return;
            }

            targetWindow.SetOccupied(true);
            appearTimer = appearDelay;
            return;
        }

        // If the window was closed whlist waiting to appear, pick another window
        if (targetWindow.IsClosed)
        {
            targetWindow.SetOccupied(false);
            targetWindow = null;
            retryTimer = 0f;
            return;
        }

        appearTimer -= Time.deltaTime;
        if (appearTimer <= 0f)
        {
            AppearInWindow(targetWindow);
        }
    }

    // While in the ActiveState, begin tracking data for appearance and siphon times 
    private void TickActive()
    {
        timeInWindow += Time.deltaTime;
        siphonTimer -= Time.deltaTime;
        if (siphonTimer <= 0f)
        {
            SetState(State.Siphoning);
        }
    }

    // While in SiphonState, siphon energy from the orb and continue tracking stats 
    private void TickSiphon()
    {
        timeInWindow += Time.deltaTime;
        energySiphoned += orb.Drain(siphonRatePerSecond * Time.deltaTime);
    }

    private void SetState(State next)
    {
        CurrentState = next; 

        switch (next)
        {
            case State.Cooldown:
                canAppear = false;
                summoned = false;
                HideSprite();
                break;
            case State.Ready:
                targetWindow = null;
                retryTimer = 0f;
                break;
            case State.Caught:
                EnterCaught();
                break; 
        }
    }

    private void EnterCaught()
    {
        var record = new AppearanceStats { timeInWindow = timeInWindow, energySiphoned = energySiphoned};
        History.Add(record);
        AppearanceEnded?.Invoke(record); 

        if (currentWindow != null)
        {
            currentWindow.SetOccupied(false);
            currentWindow.StartCooldown(windowCooldownAfterUse);
            lastWindow = currentWindow;
            currentWindow = null; 
        }

        activityCooldown = ComputeCooldown();
        SetState(State.Cooldown); 
    }
    #endregion

    #region Utility Methods
    // Check to see if the Munchkin can appear
    private bool CheckAppearance()
    {
        /// TODO: Add onto this conditional check once the Wallpaper Man is implemented in Multi-Agents
        canAppear = activityCooldown <= 0f;
        return canAppear;
    }

    // Utility : Calculates the best possible window for the Munchkin to appear in, if none found, return null
    private Window WindowToAppear()
    {
        Window best = null;
        float bestScore = -1000000f;

        foreach (var window in windows)
        {
            if (!window.IsUsable || window == lastWindow)
            {
                continue;
            }

            Vector3 position = window.AppearPoint.position;
            float score = playerDistance * FlatDistance(sensedPlayerPosition, position) + jitter * UnityEngine.Random.value;

            if (score > bestScore)
            {
                bestScore = score;
                best = window;
            }
        }

        // If the best usuable window is the last one
        if (best == null && lastWindow != null && lastWindow.IsUsable)
        {
            best = lastWindow;
        }

        return best;
    }

    // Visual : Function that handles the Munchkin's visual appearance. Also responsible from transitioning between the Ready and Active state 
    private void AppearInWindow(Window window)
    {
        currentWindow = window;
        targetWindow = null;

        // Move the gameobject to the window and then reveal the sprite
        transform.SetPositionAndRotation(window.AppearPoint.position, window.AppearPoint.rotation);
        ShowSprite();
        if (audioSource && appearCue)
        {
            audioSource.PlayOneShot(appearCue);
        }

        timeInWindow = 0f;
        energySiphoned = 0f;
        siphonTimer = siphonDelay;
        SetState(State.Active);

    }


    // Handler : Fired whenever the isCaught event is fired 
    private void HandleWindowClosed(Window window)
    {

        bool inWindow = CurrentState == State.Active || CurrentState == State.Siphoning; 
        if (inWindow && window == currentWindow)
        {
            SetState(State.Caught); 
        }
    }

    private void HandleSummoned(Window window)
    {
        // Placeholder method for Wallpaper Man implementation
    }
    #endregion

    #region Helper Methods
    private void ShowSprite()
    {
        if (spriteRenderer)
        {
            spriteRenderer.enabled = true; 
        }
    }

    private void HideSprite()
    {
        if (spriteRenderer)
        {
            spriteRenderer.enabled = false;
        }
    }

    //  Cooldown increases depending on how much the Munckin drained the orb's energy during its last appearance
    private float ComputeCooldown()
    {
        float taken = orb != null && orb.MaxEnergy > 0f ? orb.Energy / orb.MaxEnergy : 1f;
        return Mathf.Lerp(maxCooldown, minCooldown, taken); 
    }

    private void UpdatePlayerSensor()
    {
        sensorTimer -= Time.deltaTime; 
        if (sensorTimer <= 0f)
        {
            SampleSensor(); 
        }
    }

    // Calculates an estimate of the player position with noise for occasional errors 
    private void SampleSensor()
    {
        sensorTimer = sensorInterval;
        Vector2 noise = UnityEngine.Random.insideUnitCircle * sensorOffset;
        sensedPlayerPosition = player.position + new Vector3(noise.x, 0f, noise.y); 
    }

    // Distance Formula Calculation 
    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz); 
    }
    #endregion

    #region Metrics API
    public float AverageTimeInWindow()
    {
        if (History.Count == 0)
        {
            return 0f; 
        }

        float sum = 0f;
        foreach (var record in History)
        {
            sum += record.timeInWindow; 
        }

        return sum / History.Count; 
    }

    public float AverageEnergySiphoned()
    {
        {
            if (History.Count == 0)
            {
                return 0f;
            }

            float sum = 0f;

            foreach (var record in History)
            {
                sum += record.energySiphoned;
            }

            return sum / History.Count;
        }
    }
    #endregion 
}
