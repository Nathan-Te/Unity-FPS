using UnityEngine;

[RequireComponent(typeof(HeavyFPSController))]
public class PhysicsGrabber : MonoBehaviour
{
    [Header("Références")]
    public Transform playerCamera;
    public Transform holdPoint;

    [Header("Détection")]
    public LayerMask grabLayer;
    public float reachDistance = 3.0f;

    [Header("Settings Physique")]
    public float grabForce = 150f;
    public float grabDamper = 10f; // Sert maintenant à "coller" la vitesse de l'objet à celle de la main
    public float dragForce = 10f;
    public float throwForce = 15f;

    [Header("Sécurité")]
    public float breakDistance = 2.0f;

    private Rigidbody _heldRigidbody;
    private HeavyFPSController _playerController;

    // Sauvegarde des états
    private float _initialDrag;
    private float _initialAngularDrag;
    private bool _initialUseGravity;
    private CollisionDetectionMode _initialDetectionMode;

    // NOUVEAU : Pour calculer la vitesse de ton mouvement de souris
    private Vector3 _lastHoldPosition;

    public bool IsGrabbing => _heldRigidbody != null;

    void Start()
    {
        _playerController = GetComponent<HeavyFPSController>();
        if (playerCamera == null) playerCamera = _playerController.visualRoot;
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        HandleInput();
    }

    void FixedUpdate()
    {
        if (_heldRigidbody != null)
        {
            MoveObjectToHoldPoint();
        }
    }

    void HandleInput()
    {
        if (_heldRigidbody != null)
        {
            // Drop (Relâcher Clic Gauche) -> Conserve la vélocité naturelle (le fameux "Fling")
            if (Input.GetMouseButtonUp(0)) DropObject();
            // Throw (Clic Droit) -> Propulsion forcée
            else if (Input.GetMouseButtonDown(1)) ThrowObject();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) TryGrab();
        }
    }

    void TryGrab()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, reachDistance, grabLayer))
        {
            PhysicsGrabbable grabbable = hit.collider.GetComponentInParent<PhysicsGrabbable>();
            if (grabbable != null && grabbable.rb != null)
            {
                Grab(grabbable);
            }
        }
    }

    public void Grab(PhysicsGrabbable grabbable)
    {
        Rigidbody rb = grabbable.rb;

        _initialDrag = rb.linearDamping;
        _initialAngularDrag = rb.angularDamping;
        _initialUseGravity = rb.useGravity;
        _initialDetectionMode = rb.collisionDetectionMode;

        // --- NOUVEAU : On signale qu'on tient l'objet ---
        grabbable.IsHeld = true;
        // ------------------------------------------------

        if (grabbable.applyDragWhenHeld)
        {
            rb.useGravity = false;
            rb.linearDamping = dragForce;
            rb.angularDamping = dragForce;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        _lastHoldPosition = holdPoint.position;
        _heldRigidbody = rb;
        _playerController.SetCarryingState(true, grabbable.speedMultiplier, grabbable.allowSprinting);
    }

    public void DropObject()
    {
        if (_heldRigidbody == null) return;

        // --- NOUVEAU : On signale qu'on lâche ---
        PhysicsGrabbable grabbable = _heldRigidbody.GetComponent<PhysicsGrabbable>();
        if (grabbable != null) grabbable.IsHeld = false;
        // ----------------------------------------

        ClearObjectPhysics();
        _heldRigidbody = null;
        _playerController.ResetCarryingState();
    }

    void ThrowObject()
    {
        if (_heldRigidbody == null) return;

        Rigidbody rb = _heldRigidbody;
        DropObject();
        rb.AddForce(playerCamera.forward * throwForce, ForceMode.Impulse);
    }

    void MoveObjectToHoldPoint()
    {
        float distanceToHand = Vector3.Distance(_heldRigidbody.position, holdPoint.position);
        if (distanceToHand > breakDistance)
        {
            DropObject();
            return;
        }

        // --- CŒUR DU SYSTÈME ORGANIQUE ---

        // 1. Quelle est la vitesse de la main (du joueur) ?
        Vector3 holdPointVelocity = (holdPoint.position - _lastHoldPosition) / Time.fixedDeltaTime;

        // 2. Calcul des erreurs
        Vector3 errorPos = holdPoint.position - _heldRigidbody.position;
        Vector3 errorVel = holdPointVelocity - _heldRigidbody.linearVelocity;

        // 3. Application des forces (PID simplifié)
        // P (Spring) : On tire l'objet vers la position
        Vector3 springForce = errorPos * grabForce;

        // D (Damper Relatif) : On essaie de matcher la vitesse de l'objet avec celle de la main
        // C'est ça qui permet de transférer ton mouvement de souris à l'objet !
        Vector3 damperForce = errorVel * grabDamper;

        _heldRigidbody.AddForce(springForce + damperForce);

        _lastHoldPosition = holdPoint.position;
    }

    void ClearObjectPhysics()
    {
        if (_heldRigidbody != null)
        {
            _heldRigidbody.useGravity = _initialUseGravity;
            _heldRigidbody.linearDamping = _initialDrag;
            _heldRigidbody.angularDamping = _initialAngularDrag;
            _heldRigidbody.collisionDetectionMode = _initialDetectionMode;
        }
    }
}