using UnityEngine;

public class SoundFxManager : MonoBehaviour
{
    public static SoundFxManager instance;

    [SerializeField] private AudioSource SoundFxObject;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFxClip(AudioClip clip, Transform transform, float volume = 1.0f)
    {
        AudioSource audioSource = Instantiate(SoundFxObject, transform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.Play();
        audioSource.volume = volume;
        Destroy(audioSource.gameObject, clip.length);
    }

    public void PlayOnLoop(AudioClip clip, Transform transform)
    {
        AudioSource audioSource = Instantiate(SoundFxObject, transform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

}
