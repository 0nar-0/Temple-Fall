using UnityEngine;

public class Move_Audio : MonoBehaviour
{
    public AudioClip FootStepAudioClipWalk;
    public AudioClip FootStepAudioClipRun;
    public AudioClip FootStepAudioClip_Release;    
    [Range(0,1)] public float footstepaudiovolume;

    public AudioClip jumpAudioClip;
    public AudioClip landAudioClip;

    private void OnFeetsetpWalk()
    {
        SoundFXManager.Instance.PlaySound(FootStepAudioClipWalk, transform);
        SoundFXManager.Instance.PlaySound(FootStepAudioClip_Release, transform);
    }

    private void OnFeetsetpRun()
    {
        SoundFXManager.Instance.PlaySound(FootStepAudioClipRun, transform);
        SoundFXManager.Instance.PlaySound(FootStepAudioClip_Release, transform);
    }

    private void onJump()
    {
        //SoundFXManager.Instance.PlaySound(jumpAudioClip, transform);
    }

    private void onLand()
    {
        SoundFXManager.Instance.PlaySound(landAudioClip, transform);
    }
}
