using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class character_audio : MonoBehaviour
{

    public EventInstance footsteps;


    [Header("FMOD Events")]
    [SerializeField] private EventReference jumpEvent;
    [SerializeField] private EventReference landEvent;
    [SerializeField] private EventReference _footsteps;

    void Awake()
    {
        if (!_footsteps.IsNull)
        {
            footsteps = RuntimeManager.CreateInstance(_footsteps);
        }
    }

    public void onStepWalk()
    {

        RuntimeManager.AttachInstanceToGameObject(footsteps, transform, GetComponent<Rigidbody>());
        footsteps.setParameterByName("WalkRun", 0);
        footsteps.start();

    }

    public void onStepRun()
    {
        if (footsteps.isValid())
        {
            RuntimeManager.AttachInstanceToGameObject(footsteps, transform, GetComponent<Rigidbody>());
            footsteps.setParameterByName("WalkRun", 1);
            footsteps.start();
        }
    }

    public void onJump()
    {
        RuntimeManager.PlayOneShot(jumpEvent, transform.position);
    }

    public void onLand()
    {
        RuntimeManager.PlayOneShot(landEvent, transform.position);
    }


}
