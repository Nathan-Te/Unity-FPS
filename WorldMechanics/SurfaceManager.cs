using UnityEngine;
using System.Collections.Generic;

public class SurfaceManager : MonoBehaviour
{
    public static SurfaceManager Instance;

    [Header("Base de Données")]
    public List<SurfaceDefinition> surfaces;
    public SurfaceDefinition defaultSurface;

    void Awake()
    {
        // Singleton simple
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // La fonction clé : Trouve la surface à partir d'un RaycastHit
    public SurfaceDefinition GetSurfaceFromHit(RaycastHit hit)
    {
        Collider col = hit.collider;
        if (col == null) return defaultSurface;

        // 1. PRIORITÉ : Physic Material (La méthode propre)
        // On regarde si le collider a un matériau physique assigné
        if (col.sharedMaterial != null)
        {
            // Astuce : Nomme tes PhysicMaterials avec le nom du type (ex: "PM_Metal", "PM_Wood")
            // Le script cherche si le nom contient "Metal", "Wood", etc.
            string matName = col.sharedMaterial.name;

            foreach (var surf in surfaces)
            {
                if (matName.Contains(surf.surfaceType.ToString()))
                {
                    return surf;
                }
            }
        }

        // 2. SECOURS : Tag (La méthode rapide)
        // Si l'objet est tagué "Wood", on renvoie la surface Wood
        foreach (var surf in surfaces)
        {
            if (col.CompareTag(surf.surfaceType.ToString()))
            {
                return surf;
            }
        }

        // 3. PAR DÉFAUT
        return defaultSurface;
    }

    // --- API PUBLIQUE (Ce que tu appelleras depuis le Controller/Weapon) ---

    public void PlayFootstep(Vector3 position, RaycastHit groundHit, AudioSource source, float volume = 1f)
    {
        SurfaceDefinition surf = GetSurfaceFromHit(groundHit);
        if (surf == null) return;

        AudioClip clip = surf.GetRandomFootstep();
        if (clip != null && source != null)
        {
            // Variation légère de pitch pour le réalisme
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(clip, volume);
        }
    }

    public void PlayBulletImpact(Vector3 position, Vector3 normal, RaycastHit hit)
    {
        SurfaceDefinition surf = GetSurfaceFromHit(hit);
        if (surf == null) return;

        // A. Son
        AudioClip clip = surf.GetRandomImpact();
        if (clip != null)
        {
            // PlayClipAtPoint crée un objet audio temporaire
            AudioSource.PlayClipAtPoint(clip, position);
        }

        // B. Visuel (Decal)
        if (surf.impactVFX != null)
        {
            // On décale un tout petit peu le point d'impact vers l'extérieur pour éviter le clipping (Z-fighting)
            Vector3 spawnPos = position + (normal * 0.01f);

            GameObject decal = Instantiate(surf.impactVFX, spawnPos, Quaternion.LookRotation(normal));
            decal.transform.parent = hit.collider.transform; // On colle l'impact à l'objet (pour qu'il bouge avec la porte/caisse)

            Destroy(decal, 10f); // Nettoyage après 10s
        }
    }

    public SurfaceDefinition GetSurfaceFromCollider(Collider col)
    {
        if (col == null) return defaultSurface;

        if (col.sharedMaterial != null)
        {
            string matName = col.sharedMaterial.name;
            foreach (var surf in surfaces)
                if (matName.Contains(surf.surfaceType.ToString())) return surf;
        }

        foreach (var surf in surfaces)
            if (col.CompareTag(surf.surfaceType.ToString())) return surf;

        return defaultSurface;
    }
}