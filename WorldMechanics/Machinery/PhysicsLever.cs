using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(PhysicsGrabbable))] // Nécessaire pour être attrapé par ton système
public class PhysicsLever : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Le socle du levier dont on veut ignorer les collisions")]
    public GameObject socle;
    [Tooltip("L'angle auquel le levier est considéré comme ACTIVE (ex: 45)")]
    public float angleOn = 45f;
    [Tooltip("L'angle auquel le levier est considéré comme DESACTIVE (ex: 0)")]
    public float angleOff = 0f;
    [Tooltip("La marge d'erreur pour atteindre l'angle (ex: 5 degrés)")]
    public float threshold = 5f;

    [Header("Logique")]
    public bool isOneShot = false; // Si vrai, ne peut être activé qu'une seule fois (ex: alarme)

    [Header("Events")]
    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;

    [Header("Audio & FX")]
    public AudioSource audioSource;
    public AudioClip clunkSound; // Son de butée métallique

    private HingeJoint _joint;
    private bool _isActivated = false;
    private bool _wasAtLimit = false; // Pour ne jouer le son qu'une fois à l'impact

    void Start()
    {
        _joint = GetComponent<HingeJoint>();

        // Force le type "Heavy" pour que le mouvement soit lourd et ne permette pas le sprint
        var grabbable = GetComponent<PhysicsGrabbable>();
        if (grabbable) grabbable.weightType = PhysicsGrabbable.ObjectWeight.Heavy;

        if (socle != null) Physics.IgnoreCollision(socle.GetComponent<Collider>(), GetComponent<Collider>());
    }

    void Update()
    {
        // On lit l'angle actuel du joint
        float currentAngle = _joint.angle;

        // 1. DÉTECTION ACTIVATION (On tire vers l'angle ON)
        if (!_isActivated && Mathf.Abs(currentAngle - angleOn) < threshold)
        {
            Activate();
        }
        // 2. DÉTECTION DÉSACTIVATION (On remet vers l'angle OFF)
        else if (_isActivated && Mathf.Abs(currentAngle - angleOff) < threshold)
        {
            if (!isOneShot) Deactivate();
        }

        // 3. GESTION SONORE (Clunk quand on touche une butée)
        bool isAtLimit = (Mathf.Abs(currentAngle - _joint.limits.max) < 2f || Mathf.Abs(currentAngle - _joint.limits.min) < 2f);
        if (isAtLimit && !_wasAtLimit)
        {
            PlaySound();
        }
        _wasAtLimit = isAtLimit;
    }

    void Activate()
    {
        _isActivated = true;
        OnActivate.Invoke();
        Debug.Log($"Levier {name} ACTIVÉ !");
    }

    void Deactivate()
    {
        _isActivated = false;
        OnDeactivate.Invoke();
        Debug.Log($"Levier {name} DÉSACTIVÉ !");
    }

    void PlaySound()
    {
        if (audioSource && clunkSound)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clunkSound);
        }
    }

    // Gizmo pour visualiser les angles cibles dans l'éditeur
    void OnDrawGizmosSelected()
    {
        // C'est approximatif car ça dépend de l'axe du joint, mais ça aide visuellement
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(angleOn, transform.right) * transform.forward * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(angleOff, transform.right) * transform.forward * 0.5f);
    }
}