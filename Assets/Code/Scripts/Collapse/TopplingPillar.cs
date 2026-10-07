using System.Collections;
using UnityEngine;

// =====================================================================
//  OWNER: Physics & Collisions
//
//  A pillar standing on solid ground. When triggered it:
//    WARNING  : creaks and slowly leans toward its fall direction
//    COLLAPSE : becomes a real physics object and gets a push at the top,
//               so gravity topples it naturally (onto a bridge, another
//               pillar...). Add ChainReaction to whatever it should break.
//    AFTER    : when it settles, it freezes and becomes a solid obstacle
//               (or hides, or keeps simulating - your choice)
//
//  It falls toward its LOCAL BLUE ARROW (Z / forward). Rotate the pillar
//  around Y to aim it. The yellow arrow in the Scene view shows the direction.
//
//  Setup: Cylinder + CapsuleCollider (or BoxCollider) + Rigidbody + this.
//  Give the Rigidbody a high mass (e.g. 500) so it feels heavy.
// =====================================================================
[RequireComponent(typeof(Rigidbody))]
public class TopplingPillar : CollapsePiece, ISmashable
{
    public enum AfterFall { StayAsObstacle, Hide, KeepSimulating }

    [Header("Warning")]
    [Tooltip("How many degrees it leans before falling.")]
    [SerializeField] private float warnLeanAngle = 4f;
    [SerializeField] private float warnJitter = 0.6f;

    [Header("Topple")]
    [Tooltip("Speed (m/s) given to the TOP of the pillar. 1.5-3 is usually enough.")]
    [SerializeField] private float pushSpeed = 2f;
    [Tooltip("Layer while falling. Debris x Player is off, so it won't shove the player around.")]
    [SerializeField] private string debrisLayerName = "Debris";
    [Tooltip("Ignore tiny bumps when reporting impacts (for sound).")]
    [SerializeField] private float minImpactSpeedForSound = 1.5f;

    [Header("After it settles")]
    [Tooltip("KeepSimulating = safest default. StayAsObstacle only when it lands on solid ground " +
             "(if it freezes on a bridge that later breaks, it would float in the air).")]
    [SerializeField] private AfterFall afterFall = AfterFall.KeepSimulating;
    [Tooltip("Give up waiting for it to settle after this many seconds.")]
    [SerializeField] private float maxSimulationTime = 8f;

    private Rigidbody rb;
    private Collider col;
    private Vector3 startPos;
    private Quaternion startRot;
    private int startLayer;
    private Vector3 pivot;      // bottom of the pillar on the fall side
    private float height;

    private bool smashed;
    private Vector3 smashPoint, smashVelocity;
    private float lastImpactTime = -1f;

    private Vector3 FallDirection
    {
        get
        {
            Vector3 f = startRot * Vector3.forward;
            f.y = 0f;
            return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
        }
    }

    protected override void SaveStartState()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        startPos = transform.position;
        startRot = transform.rotation;
        startLayer = gameObject.layer;

        Bounds b = col != null ? col.bounds : new Bounds(transform.position, Vector3.one);
        height = b.size.y;
        // Pivot = bottom edge on the side it falls toward, so it leans like a real pillar.
        Vector3 bottomCenter = new Vector3(b.center.x, b.min.y, b.center.z);
        float radius = Mathf.Min(b.extents.x, b.extents.z);
        pivot = bottomCenter + FallDirection * radius;
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
        smashed = false;
        lastImpactTime = -1f;
    }

    public void Smash(Vector3 point, Vector3 velocityChange)
    {
        if (CurrentState == State.Collapsing || CurrentState == State.Collapsed) return;
        smashed = true;
        smashPoint = point;
        smashVelocity = velocityChange;
        CollapseNow();
    }

    protected override IEnumerator WarnRoutine(float duration)
    {
        Vector3 axis = Vector3.Cross(Vector3.up, FallDirection);   // tip-over axis
        float t = 0f;
        while (t < duration)
        {
            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
            float k = t / duration;
            float angle = warnLeanAngle * k * k + Mathf.Sin(t * 40f) * warnJitter * k;
            ApplyLean(axis, angle);
        }
    }

    private void ApplyLean(Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        rb.MovePosition(pivot + q * (startPos - pivot));
        rb.MoveRotation(q * startRot);
    }

    protected override IEnumerator CollapseRoutine()
    {
        int debris = LayerMask.NameToLayer(debrisLayerName);
        if (debris >= 0) gameObject.layer = debris;

        rb.isKinematic = false;
        rb.WakeUp();

        if (smashed)
        {
            // Knocked over by something else (domino): push where it was hit.
            rb.AddForceAtPosition(smashVelocity * rb.mass, smashPoint, ForceMode.Impulse);
        }
        else
        {
            // Triggered by AI: push the top toward the fall direction.
            Vector3 top = transform.position + transform.up * (height * 0.5f);
            rb.AddForceAtPosition(FallDirection * pushSpeed * rb.mass, top, ForceMode.Impulse);
        }

        // Let physics play it out until it settles (or we give up).
        float t = 0f;
        yield return new WaitForSeconds(0.5f);
        while (t < maxSimulationTime && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    protected override IEnumerator AfterCollapseRoutine()
    {
        switch (afterFall)
        {
            case AfterFall.StayAsObstacle:
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;                 // frozen where it landed
                gameObject.layer = startLayer;         // solid again, the player collides with it
                break;
            case AfterFall.Hide:
                gameObject.SetActive(false);
                break;
        }
        yield break;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentState != State.Collapsing) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeedForSound) return;
        if (Time.time - lastImpactTime < 0.3f) return;   // don't spam sounds
        lastImpactTime = Time.time;
        ReportImpact(collision.GetContact(0).point);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 dir = transform.forward; dir.y = 0f; dir.Normalize();
        Vector3 from = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, from + dir * 2f);
        Gizmos.DrawSphere(from + dir * 2f, 0.12f);
    }
}
