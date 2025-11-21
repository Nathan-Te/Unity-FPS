using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PhysicalControl : MonoBehaviour, IInteractable
{
    public enum ControlType { Button, Lever }

    [Header("Paramètres")]
    public ControlType type = ControlType.Button;
    public string objectName = "Bouton";

    [Header("Comportement")]
    public bool autoReset = true;
    public float resetDelay = 0.5f;

    [Header("Physique Visuelle")]
    public Transform movingPart;
    public Vector3 pressedOffset = new Vector3(0, -0.05f, 0);
    public Vector3 leverRotation = new Vector3(45f, 0, 0);
    [Tooltip("Vitesse de l'animation. 5 = Rapide (0.2s), 2 = Lourd (0.5s)")]
    public float animSpeed = 2f; // J'ai baissé la valeur par défaut pour plus de lourdeur

    [Header("Logique")]
    public bool isOneShot = false;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private bool _isOn = false;
    private bool _isAnimating = false;
    private Vector3 _initialPos;
    private Quaternion _initialRot;

    public string InteractionPrompt
    {
        get
        {
            if (type == ControlType.Button) return $"Appuyer sur {objectName}";
            return _isOn ? "Désactiver" : "Activer";
        }
    }

    void Start()
    {
        if (movingPart == null) movingPart = transform;
        _initialPos = movingPart.localPosition;
        _initialRot = movingPart.localRotation;
    }

    public bool Interact(HeavyFPSController player)
    {
        if (_isAnimating) return false;
        if (isOneShot && _isOn) return false;

        if (type == ControlType.Button && autoReset && _isOn) return false;

        _isOn = !_isOn;

        if (_isOn) onActivate.Invoke();
        else onDeactivate.Invoke();

        StartCoroutine(AnimateControl());

        return true;
    }

    IEnumerator AnimateControl()
    {
        _isAnimating = true;
        float progress = 0;

        // Calcul des cibles
        Vector3 targetPos = _isOn ? _initialPos + pressedOffset : _initialPos;
        Quaternion targetRot = _isOn ? _initialRot * Quaternion.Euler(leverRotation) : _initialRot;

        // CORRECTION CRUCIALE : On capture la rotation de départ AVANT la boucle
        Quaternion startRot = movingPart.localRotation;

        if (type == ControlType.Button)
        {
            // Phase 1 : Enfoncer
            while (progress < 1)
            {
                progress += Time.deltaTime * animSpeed;
                movingPart.localPosition = Vector3.Lerp(_initialPos, targetPos, progress); // Le bouton utilisait déjà _initialPos (fixe), donc c'était ok
                yield return null;
            }

            if (autoReset)
            {
                yield return new WaitForSeconds(resetDelay);
                _isOn = false;

                // Phase 2 : Remonter
                progress = 0;
                while (progress < 1)
                {
                    progress += Time.deltaTime * animSpeed;
                    // Pour le retour, on part de la position enfoncée (targetPos) vers l'initiale
                    movingPart.localPosition = Vector3.Lerp(targetPos, _initialPos, progress);
                    yield return null;
                }
            }
        }
        else // LEVIER
        {
            while (progress < 1)
            {
                progress += Time.deltaTime * animSpeed;

                // CORRECTION ICI : On va de 'startRot' (fixe) vers 'targetRot' (fixe)
                // Slerp est mieux pour les arcs de cercle
                movingPart.localRotation = Quaternion.Slerp(startRot, targetRot, progress);

                yield return null;
            }

            // Calage final propre pour éviter les micro-écarts
            movingPart.localRotation = targetRot;
        }

        _isAnimating = false;
    }
}