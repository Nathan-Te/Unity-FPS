using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PhysicsGrabbable))]
public class PhysicsDrawer : MonoBehaviour
{
    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip slideSound;
    public AudioClip impactSound; // Quand ça tape au fond ou s'ouvre à fond

    [Header("Réglages Audio")]
    public float minSpeedForSound = 0.1f;
    public float pitchRandomness = 0.1f;

    private Rigidbody _rb;
    private ConfigurableJoint _joint;
    private float _lastImpactTime;
    private Vector3 _limitPosition; // Pour détecter les butées

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _joint = GetComponent<ConfigurableJoint>();

        var grabbable = GetComponent<PhysicsGrabbable>();
        grabbable.weightType = PhysicsGrabbable.ObjectWeight.Heavy;
        grabbable.allowSprinting = false;

        // --- FIX : On désactive le freinage du Grabber ---
        grabbable.applyDragWhenHeld = false;
        // ------------------------------------------------

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource) audioSource.loop = true;
    }

    void Update()
    {
        HandleSound();
        CheckLimits();
    }

    void HandleSound()
    {
        if (audioSource == null || slideSound == null) return;

        // Vitesse relative sur l'axe de mouvement (souvent X local pour un ConfigurableJoint)
        float speed = Mathf.Abs(Vector3.Dot(_rb.linearVelocity, transform.right)); // Adapte l'axe si besoin

        if (speed > minSpeedForSound)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = slideSound;
                audioSource.Play();
            }

            // Pitch dynamique selon la vitesse
            audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, speed / 2f);
            audioSource.volume = Mathf.Clamp01(speed);
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == slideSound)
            {
                audioSource.Stop();
            }
        }
    }

    void CheckLimits()
    {
        // Détection d'impact simple basée sur l'arrêt brutal
        // Si on avait de la vitesse et qu'on n'en a plus -> Choc
        // (Une implémentation plus précise utiliserait OnCollisionEnter avec des butées invisibles)
    }

    // Le son d'impact est mieux géré par collision physique
    private void OnCollisionEnter(Collision collision)
    {
        // Si on tape assez fort
        if (collision.relativeVelocity.magnitude > 0.5f && impactSound)
        {
            // Évite le spam de son
            if (Time.time - _lastImpactTime > 0.2f)
            {
                audioSource.PlayOneShot(impactSound, collision.relativeVelocity.magnitude * 0.5f);
                _lastImpactTime = Time.time;
            }
        }
    }
}