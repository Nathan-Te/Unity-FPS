using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(PhysicsGrabbable))]
public class PhysicsValve : MonoBehaviour
{
    [Header("Objectif")]
    [Tooltip("Nombre de tours complets nécessaires (négatif = sens inverse)")]
    public float turnsRequired = 3.0f;
    public bool resetOnRelease = false; // Si vrai, la valve revient à 0 quand on lâche

    [Header("Feedback")]
    public Transform visualWheel; // L'objet qui tourne (si différent du collider)
    public AudioSource audioSource;
    public AudioClip squeakSound;
    public AudioClip finalThunkSound;

    [Header("Events")]
    public UnityEvent OnValveComplete;
    public UnityEvent<float> OnProgress; // Envoie 0.0 à 1.0 pour une jauge ou autre

    private HingeJoint _joint;
    private float _currentTurnAmount = 0f; // En tours (ex: 1.5 tours)
    private float _lastAngle;
    private bool _isComplete = false;

    void Start()
    {
        _joint = GetComponent<HingeJoint>();
        _lastAngle = transform.localEulerAngles.x;

        var grabbable = GetComponent<PhysicsGrabbable>();
        grabbable.weightType = PhysicsGrabbable.ObjectWeight.Heavy;

        // --- FIX : On désactive le freinage du Grabber ---
        grabbable.applyDragWhenHeld = false;
        // ------------------------------------------------

        var rb = GetComponent<Rigidbody>();
        rb.angularDamping = 2.0f;
    }

    void Update()
    {
        if (_isComplete) return;

        CalculateRotation();
        CheckCompletion();
        HandleSound();
    }

    void CalculateRotation()
    {
        // On mesure le delta de rotation depuis la dernière frame
        // Attention aux sauts de 0 à 360 degrés (Gimbal Lock partiel)

        float currentAngle = 0f;
        // On suppose que l'axe du HingeJoint est X. Change ici si c'est Y ou Z.
        if (_joint.axis.x > 0.5f) currentAngle = transform.localEulerAngles.x;
        else if (_joint.axis.y > 0.5f) currentAngle = transform.localEulerAngles.y;
        else currentAngle = transform.localEulerAngles.z;

        float delta = Mathf.DeltaAngle(_lastAngle, currentAngle);

        // Convertir en fraction de tour
        _currentTurnAmount += delta / 360f;

        _lastAngle = currentAngle;

        // Feedback progression (0 à 1)
        float progress = Mathf.Clamp01(_currentTurnAmount / turnsRequired);
        OnProgress.Invoke(progress);
    }

    void CheckCompletion()
    {
        // Si on a atteint le nombre de tours requis (dans le bon sens)
        if (Mathf.Abs(_currentTurnAmount) >= Mathf.Abs(turnsRequired))
        {
            // Vérifie si on tourne dans le bon sens (Signe identique)
            if (Mathf.Sign(_currentTurnAmount) == Mathf.Sign(turnsRequired))
            {
                Complete();
            }
        }
    }

    void Complete()
    {
        _isComplete = true;
        OnValveComplete.Invoke();
        Debug.Log("Valve Ouverte !");

        if (audioSource && finalThunkSound) audioSource.PlayOneShot(finalThunkSound);

        // Optionnel : Bloquer la valve
        // GetComponent<Rigidbody>().isKinematic = true;
    }

    void HandleSound()
    {
        // Son de grincement si ça tourne
        float angularSpeed = GetComponent<Rigidbody>().angularVelocity.magnitude;

        if (audioSource && squeakSound)
        {
            if (angularSpeed > 0.1f && !audioSource.isPlaying)
            {
                audioSource.clip = squeakSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (angularSpeed < 0.1f && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, angularSpeed / 5f);
        }
    }
}