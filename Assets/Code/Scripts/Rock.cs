using UnityEngine;

public class Rock : MonoBehaviour
{
    public AudioClip impactSound;

    void Start()
    {
        SoundFxManager.instance.PlaySoundFxClip(impactSound, transform);
    }


}
