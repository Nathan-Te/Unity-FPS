using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhysicsGrabbable))]
public class DestructiblePlank : MonoBehaviour, IDamageable
{
    [Header("Santé (Tir)")]
    public float health = 40f;

    [Header("Physique (Arrachement)")]
    public float breakForce = 500f;

    [Header("FX")]
    public GameObject splintersPrefab;
    public AudioSource audioSource;
    public AudioClip woodCrackSound;

    private FixedJoint _joint;
    private Rigidbody _rb;
    private bool _isDetached = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _joint = GetComponent<FixedJoint>();

        if (_joint == null) _joint = gameObject.AddComponent<FixedJoint>();

        _joint.breakForce = breakForce;

        var grabbable = GetComponent<PhysicsGrabbable>();
        grabbable.weightType = PhysicsGrabbable.ObjectWeight.Light;
    }

    void OnJointBreak(float breakForce)
    {
        Detach();
    }

    public void TakeDamage(float damage)
    {
        if (_isDetached) return;

        health -= damage;
        if (_rb) _rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        if (health <= 0) Detach();
    }

    void Detach()
    {
        if (_isDetached) return;
        _isDetached = true;

        if (_joint != null) Destroy(_joint);

        // --- CORRECTION ICI ---
        _rb.isKinematic = false;

        // On vérifie si le Grabber tient l'objet actuellement
        var grabbable = GetComponent<PhysicsGrabbable>();

        // Si personne ne tient l'objet (ex: détruit par un tir), on active la gravité pour qu'il tombe.
        // Si l'objet est tenu (IsHeld == true), on ne touche PAS à la gravité (le Grabber la gère déjà à false).
        if (grabbable == null || !grabbable.IsHeld)
        {
            _rb.useGravity = true;
        }
        // ----------------------

        if (audioSource && woodCrackSound) audioSource.PlayOneShot(woodCrackSound);
        if (splintersPrefab) Instantiate(splintersPrefab, transform.position, transform.rotation);
    }
}