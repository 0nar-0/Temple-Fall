using UnityEngine;

public class character_audio : MonoBehaviour
{
    [System.Serializable]
    public struct Footsteps_Walk
    {
        public string name;
        public AudioClip onStepClip;
        public AudioClip onReleaseClip;
    }

    [System.Serializable]
    public struct Footsteps_Run
    {
        public string name;
        public AudioClip onStepClip;
        public AudioClip onReleaseClip;
    }

    [System.Serializable]
    public struct JumpAudio 
    {
        public string name;
        public AudioClip onJumpClip;
        public AudioClip onLandClip;
    }


    public Footsteps_Walk[] footsteps;
    public Footsteps_Run[] footstepsRun;
    public JumpAudio[] jumpAudio;


    public void onStepWalk()
    {
        if (footsteps == null || footsteps.Length == 0)
            return;

        int randomIndex = Random.Range(0, footsteps.Length);

        SoundFxManager.instance.PlaySoundFxClip(
            footsteps[randomIndex].onStepClip,
            transform
        );
        SoundFxManager.instance.PlaySoundFxClip(
            footsteps[randomIndex].onReleaseClip,
            transform
        );
    }

    public void onStepRun()
    {
        if (footstepsRun == null || footstepsRun.Length == 0)
            return;

        int randomIndex = Random.Range(0, footstepsRun.Length);

        SoundFxManager.instance.PlaySoundFxClip(
            footstepsRun[randomIndex].onStepClip,
            transform
        );
        SoundFxManager.instance.PlaySoundFxClip(
            footstepsRun[randomIndex].onReleaseClip,
            transform
        );
    }

    public void onJump()
    {
        if (jumpAudio == null || jumpAudio.Length == 0)
            return;

        int randomIndex = Random.Range(0, jumpAudio.Length);

        SoundFxManager.instance.PlaySoundFxClip(
            jumpAudio[randomIndex].onJumpClip,
            transform
        );
    }

    public void onLand()
    {
        if (jumpAudio == null || jumpAudio.Length == 0)
            return;

        int randomIndex = Random.Range(0, jumpAudio.Length);

        SoundFxManager.instance.PlaySoundFxClip(
            jumpAudio[randomIndex].onLandClip,
            transform
        );
    }


}
