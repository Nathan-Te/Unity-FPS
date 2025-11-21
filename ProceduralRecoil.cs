using UnityEngine;

public class ProceduralRecoil : MonoBehaviour
{
    [Header("Paramètres")]
    public float snappiness = 6f; // Vitesse du "Coup" (Impact)
    public float returnSpeed = 10f; // Vitesse de retour au calme

    // Rotation
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    // Position (Kickback)
    private Vector3 currentPosition;
    private Vector3 targetPosition;

    [Header("Références")]
    // Optionnel : Si tu veux aussi secouer la caméra indépendamment
    public Transform cameraShakeNode;
    public DynamicCrosshair crosshairScript; // Assigne ton script UI ici

    void Update()
    {
        // 1. Calcul de la rotation cible (Ressort)
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);

        // 2. Calcul de la position cible (Recul vers l'arrière)
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.deltaTime);

        // 3. Application locale (On s'ajoute par-dessus le Sway)
        // Le HeavySway gère la rotation globale, nous on ajoute juste le "Shake"
        transform.localRotation = Quaternion.Euler(currentRotation);
        // Note : Si HeavySway écrase localRotation, il faudra combiner les deux scripts ou utiliser un parent intermédiaire.
        // Pour l'instant, testons le conflit.

        // Kickback (Recul de l'arme)
        transform.localPosition = currentPosition;
    }

    // Appelle cette fonction pour simuler un tir ou un choc
    public void RecoilFire(float recoilX, float recoilY, float kickBackZ)
    {
        // Ajout de l'aléatoire pour que ce soit organique
        targetRotation += new Vector3(-recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilY, recoilY));
        targetPosition += new Vector3(0, 0, -kickBackZ);

        if (crosshairScript != null)
        {
            crosshairScript.AddRecoil(150f); // Valeur arbitraire d'ouverture
        }
    }

    public void LandingImpact(float impactSpeed)
    {
        impactSpeed *= 1.5f;

        // Plus la vitesse de chute est grande, plus le choc est fort
        // On clamp pour éviter que l'arme sorte de l'écran si on tombe de très haut
        float intensity = Mathf.Clamp(Mathf.Abs(impactSpeed) * 0.3f, 1f, 15f);

        // Mouvement : L'arme s'écrase vers le bas (-Y) et un peu vers l'avant (Z) ou l'arrière
        // Rotation : Le nez de l'arme plonge violemment (X positif)

        targetPosition += new Vector3(0, -intensity * 0.05f, 0);
        targetRotation += new Vector3(intensity * 2f, Random.Range(-intensity, intensity) * 0.5f, 0);
    }
}