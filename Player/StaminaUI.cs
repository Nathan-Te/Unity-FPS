using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("Références")]
    public HeavyFPSController player;
    public Slider slider;
    public CanvasGroup canvasGroup;

    [Header("Animation")]
    public float fadeSpeed = 5f;

    void Start()
    {
        if (player == null) player = FindAnyObjectByType<HeavyFPSController>();
        if (slider == null) slider = GetComponent<Slider>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (player == null) return;

        // 1. Mettre à jour la valeur visuelle
        float currentRatio = player.CurrentStamina / player.maxStamina;
        slider.value = currentRatio;

        // 2. Gestion du Fade (Disparaît si stamina > 99%)
        float targetAlpha = (currentRatio > 0.99f) ? 0f : 1f;

        // Interpolation douce de l'opacité
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
}