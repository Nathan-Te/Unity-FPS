using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Surface System/Surface Definition")]
public class SurfaceDefinition : ScriptableObject
{
    public SurfaceType surfaceType;

    [Header("Pas (Footsteps)")]
    public List<AudioClip> footstepSounds;

    [Header("Impacts de Balles (Bullet)")]
    public GameObject impactVFX; // Le prefab de trou de balle + fumée
    public List<AudioClip> impactSounds; // Bruit de l'impact (Ricochet, Toc, Splatch)

    [Header("Impacts de Mêlée / Physique")]
    public List<AudioClip> collisionSounds; // Si une caisse tombe dessus

    // Helper pour récupérer un son au hasard
    public AudioClip GetRandomFootstep()
    {
        if (footstepSounds.Count == 0) return null;
        return footstepSounds[Random.Range(0, footstepSounds.Count)];
    }

    public AudioClip GetRandomImpact()
    {
        if (impactSounds.Count == 0) return null;
        return impactSounds[Random.Range(0, impactSounds.Count)];
    }
}