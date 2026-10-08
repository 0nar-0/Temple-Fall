using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
   public static SoundFXManager Instance;

    [SerializeField] private AudioSource audioSourcePrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PlaySound(AudioClip clip, Transform spawnPosition)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnPosition.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = Random.Range(0.6f, .9f); // Randomize volume between 0.8 and 1.0
        audioSource.pitch = Random.Range(0.8f, 1.2f); // Randomize pitch between 0.8 and 1.2
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }
}
