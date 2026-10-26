using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    #region Variables
    [Header("References")]
    [SerializeField] private Orb orb;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Layout")]
    [SerializeField] private Vector2 barSize = new Vector2(360f, 28f);
    [SerializeField] private Vector2 margin = new Vector2(40f, 40f);
    [SerializeField] private float spacing = 12f;

    [Header("Colors")]
    [SerializeField] private Color healthColor = new Color(0.85f, 0.2f, 0.2f);
    [SerializeField] private Color orbColor = new Color(0.6f, 0.25f, 0.9f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);

    private RectTransform healthFill;
    private RectTransform orbFill;
    private Text healthLabel;
    private Text orbLabel;

    private int lastHealthShown = -1;
    private int lastOrbShown = -1;
    #endregion

    #region Monobehavior Methods
    private void Awake()
    {
        BuildHud();
    }

    private void OnEnable()
    {
        if (orb != null) orb.OnEnergyChange += HandleOrbChanged;
        if (playerHealth != null) playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (orb != null) orb.OnEnergyChange -= HandleOrbChanged;
        if (playerHealth != null) playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    // Start runs after every Awake, so the orb and health already hold their starting values
    private void Start()
    {
        if (orb == null) Debug.LogWarning("[GameHUD] No Orb assigned, the orb bar will stay empty.", this);
        if (playerHealth == null) Debug.LogWarning("[GameHUD] No PlayerHealth assigned, the health bar will stay empty.", this);

        RefreshOrb();
        RefreshHealth();
    }
    #endregion

    #region Updating
    private void HandleOrbChanged(float newEnergy, float delta) => RefreshOrb();
    private void HandleHealthChanged(float current, float max) => RefreshHealth();

    private void RefreshOrb()
    {
        if (orb == null) return;
        SetBar(orbFill, orbLabel, "Orb Energy", orb.Energy, orb.MaxEnergy, ref lastOrbShown);
    }

    private void RefreshHealth()
    {
        if (playerHealth == null) return;
        SetBar(healthFill, healthLabel, "Health", playerHealth.CurrentHealth, playerHealth.MaxHealth, ref lastHealthShown);
    }

    // The fill's right edge (anchorMax.x) moves between 0 and 1 to show the amount
    private static void SetBar(RectTransform fill, Text label, string title, float current, float max, ref int lastShown)
    {
        fill.anchorMax = new Vector2(max > 0f ? Mathf.Clamp01(current / max) : 0f, 1f);

        int shown = Mathf.CeilToInt(current);
        if (shown == lastShown) return;

        lastShown = shown;
        label.text = $"{title}  {shown} / {Mathf.CeilToInt(max)}";
    }
    #endregion

    #region UI Construction
    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 18);

        // Health at the bottom, orb energy stacked above it
        CreateBar(canvasObject.transform, "Health Bar", margin, healthColor, font, out healthFill, out healthLabel);
        CreateBar(canvasObject.transform, "Orb Bar", margin + new Vector2(0f, barSize.y + spacing), orbColor, font, out orbFill, out orbLabel);
    }

    private void CreateBar(Transform parent, string barName, Vector2 position, Color fillColor, Font font,
                           out RectTransform fill, out Text label)
    {
        // Container anchored to the bottom-left corner of the screen
        RectTransform root = NewRect(barName, parent);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.zero;
        root.pivot = Vector2.zero;
        root.sizeDelta = barSize;
        root.anchoredPosition = position;

        RectTransform background = NewRect("Background", root);
        Stretch(background);
        background.gameObject.AddComponent<Image>().color = backgroundColor;

        fill = NewRect("Fill", root);
        Stretch(fill);
        fill.gameObject.AddComponent<Image>().color = fillColor;

        RectTransform labelRect = NewRect("Label", root);
        Stretch(labelRect);
        label = labelRect.gameObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = 18;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private static RectTransform NewRect(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
    #endregion
}
