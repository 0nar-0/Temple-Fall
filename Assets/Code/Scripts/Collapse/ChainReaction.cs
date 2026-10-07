using System.Collections;
using UnityEngine;

// =====================================================================
//  OWNER: Physics & Collisions
//
//  Add next to any CollapsePiece (platform, pillar...) to make destruction
//  spread "naturally". Two independent features:
//
//  1) BREAK ON IMPACT  (the physics part - looks natural)
//     When a moving physics object (falling pillar, tumbling debris) hits
//     this piece hard enough, this piece breaks too and gets pushed in the
//     direction of the hit.
//
//  2) NEXT PIECES  (the scripted part - guarantees it happens)
//     When this piece collapses (for ANY reason), it also collapses the
//     pieces in "Next" after small delays. Use it to:
//       - "unzip" a bridge: segment 5 breaks -> 4 and 6 follow 0.15 s later
//       - make sure a domino chain always finishes even if physics misses
//     A piece that's already falling ignores this, so having both
//     (impact + backup link) is safe.
// =====================================================================
[RequireComponent(typeof(CollapsePiece))]
public class ChainReaction : MonoBehaviour
{
    [System.Serializable]
    public class Link
    {
        public CollapsePiece piece;
        [Tooltip("Seconds after THIS piece starts collapsing.")]
        public float delay = 0.15f;
        [Tooltip("Off = breaks instantly (looks like a chain reaction). On = shakes first.")]
        public bool withWarning = false;
    }

    [Header("1) Break when something hits me")]
    [SerializeField] private bool breakOnImpact = true;
    [Tooltip("How fast (m/s) the hitting object must be moving to break this piece.")]
    [SerializeField] private float minImpactSpeed = 2f;
    [Tooltip("How much of the hit's speed is passed on as a push.")]
    [SerializeField] private float pushMultiplier = 0.5f;

    [Header("2) When I collapse, also collapse these")]
    [SerializeField] private Link[] next;

    private CollapsePiece self;

    private void Awake()
    {
        self = GetComponent<CollapsePiece>();
        CollapseBus.PieceCollapsed += OnPieceCollapsed;
        CollapseBus.AllReset += OnReset;
    }

    private void OnDestroy()
    {
        CollapseBus.PieceCollapsed -= OnPieceCollapsed;
        CollapseBus.AllReset -= OnReset;
    }

    private void OnReset() => StopAllCoroutines();

    // ---------- 1) Break on impact ----------
    private void OnCollisionEnter(Collision collision)
    {
        if (!breakOnImpact) return;
        if (self.CurrentState == CollapsePiece.State.Collapsing ||
            self.CurrentState == CollapsePiece.State.Collapsed) return;

        // Only real moving physics objects break things.
        // The player (CharacterController) has no Rigidbody, so walking on it never breaks it.
        Rigidbody other = collision.rigidbody;
        if (other == null || other.isKinematic) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        Vector3 point = collision.GetContact(0).point;
        Vector3 dir = other.GetPointVelocity(point);
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.down;
        Vector3 push = dir.normalized * speed * pushMultiplier;

        if (self is ISmashable smashable) smashable.Smash(point, push);
        else self.CollapseNow();
    }

    // ---------- 2) Next pieces ----------
    private void OnPieceCollapsed(CollapsePiece piece)
    {
        if (piece != self || next == null || !isActiveAndEnabled) return;
        foreach (Link link in next)
            if (link.piece != null) StartCoroutine(Fire(link));
    }

    private IEnumerator Fire(Link link)
    {
        if (link.delay > 0f) yield return new WaitForSeconds(link.delay);
        if (link.withWarning) link.piece.Trigger();
        else link.piece.CollapseNow();
    }

    // Draw lines to the "next" pieces in the Scene view (select the object to see them).
    private void OnDrawGizmosSelected()
    {
        if (next == null) return;
        Gizmos.color = Color.magenta;
        foreach (Link link in next)
            if (link.piece != null) Gizmos.DrawLine(transform.position, link.piece.transform.position);
    }
}
