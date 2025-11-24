using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsGrabbable : MonoBehaviour
{
    public enum ObjectWeight { Light, Heavy }

    [Header("Infos Objet")]
    public ObjectWeight weightType = ObjectWeight.Light;

    [Header("Comportement Grab")]
    [Tooltip("Si VRAI : On coupe la gravité et on augmente la friction quand tenu (Caisse). Si FAUX : On laisse la physique gérer (Valve, Tiroir).")]
    public bool applyDragWhenHeld = true; // NOUVEAU : Par défaut True pour les objets standards

    [Header("Impact sur le Joueur")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.9f;
    public bool allowSprinting = true;

    [HideInInspector] public bool IsHeld = false;

    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupPhysics();
    }

    void SetupPhysics()
    {
        if (weightType == ObjectWeight.Heavy)
        {
            rb.mass = 20f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.15f;

            speedMultiplier = 0.5f;
            allowSprinting = false;
        }
        else
        {
            rb.mass = 2f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.15f;

            speedMultiplier = 0.9f;
            allowSprinting = true;
        }

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }
}