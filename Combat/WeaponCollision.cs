using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("L'objet qui va bouger (doit être un parent de l'arme)")]
    public Transform weaponPivot;
    public Transform cameraTransform;

    [Header("Détection")]
    public float checkDistance = 1.0f;
    public LayerMask collisionLayer; // Les murs (Default, Ground...)

    [Header("Réaction Rotation")]
    public Vector3 blockedRotation = new Vector3(-60f, 0f, 0f); // L'arme se lève vers le ciel (X négatif)

    [Header("Réaction Position")]
    [Tooltip("Décalage local quand bloqué (ex: Z = -0.3 pour reculer l'arme vers le joueur)")]
    public Vector3 blockedPositionOffset = new Vector3(0f, -0.1f, -0.3f);

    [Header("Fluidité")]
    public float reactionSpeed = 10f;

    // Propriété publique pour bloquer le tir
    public bool IsBlocked { get; private set; }

    private Quaternion _targetRotation;
    private Quaternion _originalRotation;
    private Vector3 _originalPosition;

    void Start()
    {
        if (weaponPivot == null) weaponPivot = transform;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        // On sauvegarde l'état initial (Position/Rotation de repos)
        _originalRotation = weaponPivot.localRotation;
        _originalPosition = weaponPivot.localPosition;
    }

    void Update()
    {
        CheckWall();
        AnimateWeapon();
    }

    void CheckWall()
    {
        // On part de la caméra
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // On vérifie si on touche un mur
        if (Physics.Raycast(ray, checkDistance, collisionLayer))
        {
            IsBlocked = true;
        }
        else
        {
            IsBlocked = false;
        }
    }

    void AnimateWeapon()
    {
        // 1. Calcul des Cibles
        Quaternion rotGoal;
        Vector3 posGoal;

        if (IsBlocked)
        {
            rotGoal = Quaternion.Euler(blockedRotation);
            posGoal = _originalPosition + blockedPositionOffset; // On applique l'offset
        }
        else
        {
            rotGoal = _originalRotation;
            posGoal = _originalPosition;
        }

        // 2. Interpolation Fluide
        weaponPivot.localRotation = Quaternion.Slerp(weaponPivot.localRotation, rotGoal, Time.deltaTime * reactionSpeed);
        weaponPivot.localPosition = Vector3.Lerp(weaponPivot.localPosition, posGoal, Time.deltaTime * reactionSpeed);
    }

    void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = IsBlocked ? Color.red : Color.green;
            Gizmos.DrawLine(cameraTransform.position, cameraTransform.position + cameraTransform.forward * checkDistance);
        }
    }
}