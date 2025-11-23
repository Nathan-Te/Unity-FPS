using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsGrabbable : MonoBehaviour
{
    public enum ObjectWeight { Light, Heavy }

    [Header("Infos Objet")]
    public ObjectWeight weightType = ObjectWeight.Light;

    [Header("Impact sur le Joueur")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.9f;
    public bool allowSprinting = true;

    [HideInInspector] public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupPhysics();
    }

    void SetupPhysics()
    {
        // --- CORRECTION PHYSIQUE SÈCHE ---

        // Pour éviter que l'objet ne glisse indéfiniment au sol, 
        // il vaut mieux utiliser un "Physic Material" (Friction) sur le Collider
        // plutôt que d'augmenter le Damping (qui freine aussi dans les airs).

        if (weightType == ObjectWeight.Heavy)
        {
            rb.mass = 20f; // Plus lourd (20kg)
            rb.linearDamping = 0.05f; // Très peu de résistance à l'air (ne flotte pas)
            rb.angularDamping = 0.15f; // Tourne longtemps

            speedMultiplier = 0.5f;
            allowSprinting = false;
        }
        else
        {
            rb.mass = 2f; // Standard (2kg)
            rb.linearDamping = 0.05f; // Chute rapide
            rb.angularDamping = 0.15f; // Rotation libre

            speedMultiplier = 0.9f;
            allowSprinting = true;
        }

        // Optionnel : Pour éviter que les objets passent à travers les murs lors des lancers rapides
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }
}