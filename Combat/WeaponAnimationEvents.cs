using UnityEngine;

public class WeaponAnimationEvents : MonoBehaviour
{
    public AudioSource audioSource;

    // Appelle cette fonction depuis l'onglet "Animation" d'Unity via un Event
    public void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Tu pourras ajouter d'autres events ici (ex: EjectShell, CameraShakeSpecific, etc.)
}