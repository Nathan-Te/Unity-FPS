using UnityEngine;

public class HeavySway : MonoBehaviour
{
    [Header("Références")]
    public HeavyFPSController playerController;

    [Header("Rotation Sway (Lag souris)")]
    public float rotationAmount = 4f;
    public float maxRotationAmount = 5f;
    public float rotationSmooth = 12f;

    [Header("Movement Sway (Inertie déplacement)")]
    public float moveSwayAmount = 0.05f;
    public float moveSwaySmooth = 10f;

    [Header("Tilt (Inclinaison Strafe)")]
    public float tiltAmount = 3f;
    public float tiltSmooth = 8f;

    [Header("Breathing (Respiration)")]
    public float breathAmount = 0.01f; // Amplitude de base
    public float breathSpeed = 1.5f;   // Vitesse de base

    // Position/Rotation initiales
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Accumulateur pour éviter les sauts de phase quand la vitesse change
    private float _breathTimer;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        if (playerController == null)
            playerController = GetComponentInParent<HeavyFPSController>();
    }

    void Update()
    {
        CalculateSway();
    }

    void CalculateSway()
    {
        // 1. FREE AIM
        float aimX = playerController.CurrentFreeAimX;
        float aimY = playerController.CurrentFreeAimY;
        Quaternion targetRotationFreeAim = Quaternion.Euler(aimY, aimX, 0);

        // 2. TILT
        float moveX = Input.GetAxisRaw("Horizontal");
        Quaternion targetTilt = Quaternion.Euler(0, 0, -moveX * tiltAmount);

        // COMBINAISON ROTATION
        Quaternion targetRotation = initialRotation * targetRotationFreeAim * targetTilt;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmooth);

        // 3. POSITION SWAY
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 targetPosition = initialPosition;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            targetPosition += new Vector3(-moveX * moveSwayAmount, -Mathf.Abs(moveZ) * moveSwayAmount, -moveSwayAmount);
        }

        // --- GESTION FATIGUE & RESPIRATION ---
        float fatigueFactor = 0;
        if (playerController.maxStamina > 0)
        {
            fatigueFactor = 1.0f - (playerController.CurrentStamina / playerController.maxStamina);
        }

        // Respiration (Verticale)
        float currentBreathSpeed = breathSpeed + (fatigueFactor * 3.0f); // Un peu moins rapide qu'avant
        float currentBreathAmount = breathAmount + (fatigueFactor * 0.02f);

        _breathTimer += Time.deltaTime * currentBreathSpeed;
        float breathY = Mathf.Sin(_breathTimer) * currentBreathAmount;
        targetPosition.y += breathY;

        // --- TREMBLEMENT ORGANIQUE (CORRIGÉ) ---
        if (fatigueFactor > 0.3f) // On commence un peu plus tôt mais doucement
        {
            // On adoucit l'intensité max (0.03f au lieu de 0.05f) pour que ce soit subtil
            float shakeIntensity = (fatigueFactor - 0.3f) * 0.03f;

            // --- CORRECTION ICI : VITESSE ---
            // On passe de * 20f (vibration) à * 4f (dérive lente/lourde)
            // Cela donne l'impression que l'arme est trop lourde à porter
            float noiseSpeed = 4.0f;

            // On utilise des offsets différents pour X et Y pour que le mouvement ne soit pas diagonal
            float shakeX = (Mathf.PerlinNoise(Time.time * noiseSpeed, 0f) - 0.5f) * 2f;
            float shakeY = (Mathf.PerlinNoise(100f, Time.time * noiseSpeed) - 0.5f) * 2f;

            targetPosition.x += shakeX * shakeIntensity;
            targetPosition.y += shakeY * shakeIntensity;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * moveSwaySmooth);
    }
}