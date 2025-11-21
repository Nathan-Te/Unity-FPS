using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SimpleElevator : MonoBehaviour
{
    [Header("Configuration")]
    public List<Transform> floors;
    public float travelTime = 1.5f; // Temps approximatif pour changer d'étage (plus c'est bas, plus c'est rapide)
    public int currentFloorIndex = 0;

    private bool _isMoving = false;
    private Vector3 _targetPosition;
    private Rigidbody _rb;

    // Variables pour le lissage (SmoothDamp)
    private Vector3 _currentVelocity; // Vitesse interne gérée par Unity

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        if (floors.Count > 0)
        {
            _rb.position = floors[currentFloorIndex].position;
            _targetPosition = _rb.position;
        }
    }

    public void GoToFloor(int floorIndex)
    {
        // On permet de changer de cible même en mouvement pour plus de fluidité
        if (floorIndex < 0 || floorIndex >= floors.Count) return;

        currentFloorIndex = floorIndex;
        _targetPosition = floors[floorIndex].position;
        _isMoving = true;
    }

    public void CallElevatorTo(Transform buttonLocation)
    {
        int nearestIndex = 0;
        float minDst = float.MaxValue;
        for (int i = 0; i < floors.Count; i++)
        {
            float dst = Vector3.Distance(floors[i].position, buttonLocation.position);
            if (dst < minDst) { minDst = dst; nearestIndex = i; }
        }
        GoToFloor(nearestIndex);
    }

    void FixedUpdate()
    {
        // On exécute la logique tant qu'on n'est pas EXACTEMENT à la cible
        // ou tant qu'il reste de la vitesse résiduelle
        if (_isMoving || _currentVelocity.magnitude > 0.01f)
        {
            // SmoothDamp : La fonction magique qui gère accélération et freinage
            Vector3 newPos = Vector3.SmoothDamp(
                _rb.position,
                _targetPosition,
                ref _currentVelocity,
                travelTime * 0.3f, // Temps de lissage (SmoothTime)
                10f, // Vitesse Max
                Time.fixedDeltaTime
            );

            _rb.MovePosition(newPos);

            // Vérification d'arrivée
            // On vérifie la distance ET la vitesse (pour être sûr qu'on est bien arrêté)
            if (Vector3.Distance(_rb.position, _targetPosition) < 0.01f && _currentVelocity.magnitude < 0.01f)
            {
                _rb.MovePosition(_targetPosition); // Calage final
                _isMoving = false;
            }
        }
    }
}