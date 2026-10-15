using UnityEngine;

public class CollapseFxSound : MonoBehaviour
{
    public AudioClip collapseSoundloop;

    public AudioClip collapseSound;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundFxManager.instance.PlayOnLoop(collapseSoundloop, transform);
    }

    void StartCollapseSound()
    {
        SoundFxManager.instance.PlayOnLoop(collapseSound, transform);
    }

}
