using System.Collections;
using UnityEngine;

// =====================================================================
//  OWNER: Physics & Collisions
//
//  A floor piece that shakes (warning), drops a fixed distance the same
//  way every run (scripted = reliable), and then optionally turns into a
//  real physics object that tumbles (just for show).
//
//  Setup: Cube + BoxCollider + Rigidbody + this script. Give it an ID.
// =====================================================================
//  If something HITS it (see ChainReaction), it is "smashed": no warning,
//  no scripted drop - it goes straight to physics, pushed by the hit.
[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : CollapsePiece, ISmashable
{
    [Header("Warning shake")]
    [SerializeField] private float shakeAmount = 0.06f;

    [Header("Scripted fall (same every run)")]
    [SerializeField] private float scriptedFallDistance = 3f;
    [SerializeField] private float fallGravity = 25f;   // same as the player's gravity, so it feels natural

    [Header("After the scripted fall")]
    [Tooltip("Turn into a real tumbling physics object after the scripted drop (only for looks).")]
    [SerializeField] private bool tumbleWithPhysicsAfter = true;
    [Tooltip("Layer used while tumbling. Set the Layer Collision Matrix so Debris does NOT collide with Player.")]
    [SerializeField] private string debrisLayerName = "Debris";
    [SerializeField] private float hideAfterSeconds = 4f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private int startLayer;
    private bool impacted;

    private bool smashed;
    private Vector3 smashPoint, smashVelocity;

    public void Smash(Vector3 point, Vector3 velocityChange)
    {
        if (CurrentState == State.Collapsing || CurrentState == State.Collapsed) return;
        smashed = true;
        smashPoint = point;
        smashVelocity = velocityChange;
        CollapseNow();
    }

    protected override void SaveStartState()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        startPos = transform.position;
        startRot = transform.rotation;
        startLayer = gameObject.layer;
    }

    protected override void RestoreStartState()
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        transform.SetPositionAndRotation(startPos, startRot);
        rb.position = startPos;
        rb.rotation = startRot;
        gameObject.layer = startLayer;
        impacted = false;
        smashed = false;
    }

    protected override IEnumerator WarnRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;

            float strength = shakeAmount * (t / duration);   // shake gets stronger
            Vector3 offset = Random.insideUnitSphere * strength;
            offset.y = 0f;                                    // only sideways, so the player doesn't bounce
            rb.MovePosition(startPos + offset);
        }
        rb.MovePosition(startPos);
    }

    protected override IEnumerator CollapseRoutine()
    {
        if (smashed)
        {
            // Hit by something: skip the scripted drop, go straight to physics.
            int debrisLayer = LayerMask.NameToLayer(debrisLayerName);
            if (debrisLayer >= 0) gameObject.layer = debrisLayer;
            rb.isKinematic = false;
            rb.AddForceAtPosition(smashVelocity * rb.mass, smashPoint, ForceMode.Impulse);
            yield break;
        }

        float speed = 0f;
        float fallen = 0f;

        while (fallen < scriptedFallDistance)
        {
            yield return new WaitForFixedUpdate();
            speed += fallGravity * Time.fixedDeltaTime;
            float step = speed * Time.fixedDeltaTime;
            fallen += step;
            rb.MovePosition(rb.position + Vector3.down * step);
        }

        if (tumbleWithPhysicsAfter)
        {
            int debris = LayerMask.NameToLayer(debrisLayerName);
            if (debris >= 0) gameObject.layer = debris;

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.down * speed;
            rb.angularVelocity = Random.insideUnitSphere * 2f;
        }
    }

    protected override IEnumerator AfterCollapseRoutine()
    {
        if (hideAfterSeconds <= 0f) yield break;
        yield return new WaitForSeconds(hideAfterSeconds);
        gameObject.SetActive(false);   // ResetPiece() turns it back on
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impacted || CurrentState == State.Idle || CurrentState == State.Warning) return;
        impacted = true;
        ReportImpact(collision.GetContact(0).point);
    }
}
