using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("Références")]
    public Transform weaponMuzzle; // Le bout du canon (Cube)
    public Camera mainCamera;

    [Header("Les 4 parties")]
    public RectTransform topPart;
    public RectTransform bottomPart;
    public RectTransform leftPart;
    public RectTransform rightPart;

    [Header("Réglages Dispersion")]
    public float baseSpread = 20f; // METS 20 ICI DANS L'INSPECTEUR
    public float maxSpread = 60f;
    public float spreadRecovery = 5f;

    [Header("Visée")]
    public float aimSpreadFactor = 0.4f;
    private bool _isAiming = false;

    private float _currentSpread;
    private float _addSpreadAmount;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        _currentSpread = baseSpread;
    }

    void Update()
    {
        HandlePosition();
        HandleSpread();
    }

    void HandlePosition()
    {
        // Si pas d'arme assignée, on reste au centre de l'écran (fallback)
        if (weaponMuzzle == null) return;

        Vector3 targetPoint = weaponMuzzle.position + weaponMuzzle.forward * 20f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPoint);

        // Si le point est devant la caméra
        if (screenPos.z > 0)
        {
            transform.position = screenPos;
        }
    }

    void HandleSpread()
    {
        float targetSpread = baseSpread + _addSpreadAmount;

        // --- MODIFICATION ICI ---
        if (_isAiming)
        {
            targetSpread *= aimSpreadFactor;
        }
        // ------------------------

        _currentSpread = Mathf.Lerp(_currentSpread, targetSpread, Time.deltaTime * 15f);
        _addSpreadAmount = Mathf.Lerp(_addSpreadAmount, 0, Time.deltaTime * spreadRecovery);

        // ... (Application aux positions inchangée) ...
        if (topPart) topPart.anchoredPosition = new Vector2(0, _currentSpread);
        if (bottomPart) bottomPart.anchoredPosition = new Vector2(0, -_currentSpread);
        if (leftPart) leftPart.anchoredPosition = new Vector2(-_currentSpread, 0);
        if (rightPart) rightPart.anchoredPosition = new Vector2(_currentSpread, 0);
    }

    public void SetAiming(bool state)
    {
        _isAiming = state;
    }

    public void AddRecoil(float amount)
    {
        _addSpreadAmount += amount;
        // On plafonne pour ne pas que le viseur sorte de l'écran
        if (_addSpreadAmount > maxSpread) _addSpreadAmount = maxSpread;
    }
}