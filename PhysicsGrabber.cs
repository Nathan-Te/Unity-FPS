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
    public float grabDamper = 10f;
    public float dragForce = 10f;
    public float throwForce = 15f;
    // J'ai retiré "throwSpin" des variables car inutile

    [Header("Sécurité")]
    public float breakDistance = 2.0f;

    private Rigidbody _heldRigidbody;
    private HeavyFPSController _playerController;

    private float _initialDrag;
    private float _initialAngularDrag;
    private bool _initialUseGravity;
    private CollisionDetectionMode _initialDetectionMode;

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
            // Relâcher (Clic Gauche UP)
            if (Input.GetMouseButtonUp(0)) DropObject();
            // Lancer (Clic Droit DOWN)
            else if (Input.GetMouseButtonDown(1)) ThrowObject();
        }
        else
        {
            // Attraper (Clic Gauche DOWN)
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

        rb.useGravity = false;
        rb.linearDamping = dragForce;
        rb.angularDamping = dragForce;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _heldRigidbody = rb;
        _playerController.SetCarryingState(true, grabbable.speedMultiplier, grabbable.allowSprinting);
    }

    public void DropObject()
    {
        if (_heldRigidbody == null) return;

        ClearObjectPhysics();
        _heldRigidbody = null;
        _playerController.ResetCarryingState();
    }

    void ThrowObject()
    {
        if (_heldRigidbody == null) return;

        Rigidbody rb = _heldRigidbody;

        // On lâche l'objet proprement (remet la gravité, etc.)
        DropObject();

        // --- PROPULSION PURE ---
        // Aucune rotation ajoutée ("AddTorque" supprimé).
        // L'objet partira droit et ne tournera que s'il percute un mur/sol.
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

        Vector3 direction = holdPoint.position - _heldRigidbody.position;
        Vector3 targetVelocity = direction * grabForce;
        Vector3 force = targetVelocity - _heldRigidbody.linearVelocity * grabDamper;

        _heldRigidbody.AddForce(force);
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