using UnityEngine;

public class ProceduralArmStates : MonoBehaviour
{
    [Header("Références")]
    public HeavyFPSController player; // Pour savoir si on sprint/carry

    [Header("Pose Sprint (Tactical Tuck)")]
    public Vector3 sprintPos; // Position de l'arme quand on court
    public Vector3 sprintRot; // Rotation (ex: levée vers le haut ou baissée)

    [Header("Pose Carry (Portage Objet)")]
    public Vector3 carryPos; // Position basse (pour dégager la vue)
    public Vector3 carryRot;

    [Header("Réglages")]
    public float transitionSpeed = 6f; // Vitesse de transition entre les poses

    // État de base (Tir / Idle) - Capturé au démarrage
    private Vector3 _defaultPos;
    private Quaternion _defaultRot;

    // Cibles actuelles
    private Vector3 _targetPos;
    private Quaternion _targetRot;

    void Start()
    {
        // On capture la position que tu as réglée dans l'éditeur comme "Position de base"
        _defaultPos = transform.localPosition;
        _defaultRot = transform.localRotation;

        if (player == null) player = GetComponentInParent<HeavyFPSController>();
    }

    void Update()
    {
        HandleStates();
        ApplyTransform();
    }

    void HandleStates()
    {
        // Priorité 1 : Porter un objet (Mains baissées ou cachées)
        // Note : Il faudra rendre la propriété IsCarrying publique dans HeavyFPSController
        if (player.IsCarrying)
        {
            _targetPos = carryPos;
            _targetRot = Quaternion.Euler(carryRot);
        }
        // Priorité 2 : Sprinter (Arme contre le torse)
        else if (player.IsSprinting)
        {
            _targetPos = sprintPos;
            _targetRot = Quaternion.Euler(sprintRot);
        }
        // Défaut : Arme prête
        else
        {
            _targetPos = _defaultPos;
            _targetRot = _defaultRot;
        }
    }

    void ApplyTransform()
    {
        // Interpolation fluide (Lourdeur)
        // On utilise Lerp pour aller doucement vers la pose cible
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, Time.deltaTime * transitionSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRot, Time.deltaTime * transitionSpeed);
    }
}