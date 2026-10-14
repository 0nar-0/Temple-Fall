using UnityEngine;

public class character_audio : MonoBehaviour
{
    public AudioSource move;
    public AudioSource jump;

    void onStep()
    {
        Debug.Log("Step");
        move.Play();
    }

    void onJump()
    {
        Debug.Log("Jump");
        jump.Play();
    }
}
