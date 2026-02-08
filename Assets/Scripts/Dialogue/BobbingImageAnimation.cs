using UnityEngine;

/// <summary>
/// Creates a smooth bobbing animation effect for UI Images and Sprites
/// Professional implementation with multiple animation types and customization options
/// </summary>
public class BobbingImageAnimation : MonoBehaviour
{
    [System.Serializable]
    public enum BobType
    {
        Vertical,
        Horizontal,
        Circular,
        Scale
    }

    [Header("Animation Settings")]
    [SerializeField] private BobType animationType = BobType.Vertical;
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Rotation Settings (Circular Only)")]
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Scale Settings (Scale Only)")]
    [SerializeField] private Vector3 scaleAmplitude = new Vector3(0.2f, 0.2f, 0f);

    [Header("Phase Offset")]
    [SerializeField] private float phaseOffset = 0f;

    [Header("Advanced Settings")]
    [SerializeField] private AnimationCurve bobCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool randomizePhase = true;

    // Private variables
    private Vector3 startPosition;
    private Vector3 startScale;
    private RectTransform rectTransform;
    private Transform objectTransform;
    private float randomPhase;
    private bool isUIElement;

    void Awake()
    {
        InitializeComponents();
        CacheStartValues();

        if (randomizePhase)
        {
            randomPhase = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void OnEnable()
    {
        // Reset to start position when enabled
        if (isUIElement && rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
        else if (objectTransform != null)
        {
            objectTransform.localPosition = startPosition;
        }
    }

    private void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        isUIElement = rectTransform != null;

        if (!isUIElement)
        {
            objectTransform = transform;
        }
    }

    private void CacheStartValues()
    {
        if (isUIElement)
        {
            startPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }
        else
        {
            startPosition = transform.localPosition;
            startScale = transform.localScale;
        }
    }

    void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float animationTime = (time * frequency) + phaseOffset + randomPhase;

        ApplyBobbingAnimation(animationTime);
    }

    private void ApplyBobbingAnimation(float time)
    {
        float bobValue = Mathf.Sin(time);
        float curvedBobValue = bobCurve.Evaluate((bobValue + 1f) * 0.5f) * 2f - 1f; // Remap from 0-1 to -1 to 1

        Vector3 newPosition = startPosition;
        Vector3 newScale = startScale;

        switch (animationType)
        {
            case BobType.Vertical:
                newPosition.y += curvedBobValue * amplitude;
                break;

            case BobType.Horizontal:
                newPosition.x += curvedBobValue * amplitude;
                break;

            case BobType.Circular:
                newPosition.x += Mathf.Cos(time) * amplitude;
                newPosition.y += curvedBobValue * amplitude;
                break;

            case BobType.Scale:
                newScale += scaleAmplitude * curvedBobValue;
                break;
        }

        // Apply the calculated values
        if (isUIElement)
        {
            rectTransform.anchoredPosition = newPosition;

            if (animationType == BobType.Scale)
            {
                rectTransform.localScale = newScale;
            }

            // Apply rotation for circular animation
            if (animationType == BobType.Circular)
            {
                rectTransform.localEulerAngles = new Vector3(0, 0, time * rotationSpeed);
            }
        }
        else
        {
            objectTransform.localPosition = newPosition;

            if (animationType == BobType.Scale)
            {
                objectTransform.localScale = newScale;
            }

            // Apply rotation for circular animation
            if (animationType == BobType.Circular)
            {
                objectTransform.localEulerAngles = new Vector3(0, 0, time * rotationSpeed);
            }
        }
    }

    #region Public Methods
    /// <summary>
    /// Change animation type at runtime
    /// </summary>
    public void SetAnimationType(BobType newType)
    {
        animationType = newType;
        CacheStartValues(); // Re-cache start values when changing animation type
    }

    /// <summary>
    /// Set new amplitude for the bobbing animation
    /// </summary>
    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }

    /// <summary>
    /// Set new frequency for the bobbing animation
    /// </summary>
    public void SetFrequency(float newFrequency)
    {
        frequency = newFrequency;
    }

    /// <summary>
    /// Reset the object to its original position and scale
    /// </summary>
    public void ResetToStart()
    {
        CacheStartValues();

        if (isUIElement && rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localScale = startScale;
        }
        else if (objectTransform != null)
        {
            objectTransform.localPosition = startPosition;
            objectTransform.localScale = startScale;
        }
    }
    #endregion

    #region Editor Support
#if UNITY_EDITOR
    void OnValidate()
    {
        // Clamp values to prevent negative values where inappropriate
        amplitude = Mathf.Max(0, amplitude);
        frequency = Mathf.Max(0, frequency);

        if (bobCurve == null || bobCurve.length < 2)
        {
            bobCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
#endif
    #endregion
}