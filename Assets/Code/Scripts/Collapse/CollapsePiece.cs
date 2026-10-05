using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// =====================================================================
//  SHARED CONTRACT - base class for everything that can collapse.
//
//  Every destructible thing (platform, pillar, wall, ceiling...) inherits
//  from this. It always goes through the same steps:
//
//     Idle  ->  Warning (telegraph)  ->  Collapsing  ->  Collapsed
//
//  Physics writes the subclasses (HOW it shakes / falls).
//  AI only needs: the ID, Trigger(), CollapseNow(), ResetPiece().
// =====================================================================
public abstract class CollapsePiece : MonoBehaviour
{
    public enum State { Idle, Warning, Collapsing, Collapsed }

    [Header("Collapse ID (the AI uses this name)")]
    [SerializeField] private string id;

    [Tooltip("How long the warning (shake, dust, sound) lasts before the real collapse. Agree on this with AI + Audio.")]
    [SerializeField] protected float warnDuration = 0.8f;

    [Header("Optional hooks for Audio / VFX (drag things in the inspector)")]
    public UnityEvent onWarn;
    public UnityEvent onCollapse;
    public UnityEvent onImpact;

    public string Id => id;
    public float WarnDuration => warnDuration;
    public State CurrentState { get; private set; } = State.Idle;

    private Coroutine running;

    protected virtual void Awake()
    {
        SaveStartState();
        CollapseBus.Register(this);
    }

    // Unregister only on destroy (not on disable), so hidden pieces still get reset on restart.
    protected virtual void OnDestroy() => CollapseBus.Unregister(this);

    /// Warn first, then collapse. Ignored if already started.
    public void Trigger()
    {
        if (CurrentState != State.Idle || !isActiveAndEnabled) return;
        running = StartCoroutine(Run(true));
    }

    /// Skip the warning (or cut it short) and collapse right now.
    public void CollapseNow()
    {
        if (CurrentState == State.Collapsing || CurrentState == State.Collapsed || !isActiveAndEnabled) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Run(false));
    }

    /// Back to the start state (used on player death / restart).
    public void ResetPiece()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (running != null) StopCoroutine(running);
        running = null;
        StopAllCoroutines();
        CurrentState = State.Idle;
        RestoreStartState();
    }

    private IEnumerator Run(bool warnFirst)
    {
        if (warnFirst && warnDuration > 0f)
        {
            CurrentState = State.Warning;
            onWarn?.Invoke();
            CollapseBus.RaiseWarned(this);
            yield return WarnRoutine(warnDuration);
        }

        CurrentState = State.Collapsing;
        onCollapse?.Invoke();
        CollapseBus.RaiseCollapsed(this);
        yield return CollapseRoutine();

        CurrentState = State.Collapsed;
        running = null;
        yield return AfterCollapseRoutine();
    }

    /// Call from a subclass when the piece hits something (for sound, dust, camera shake).
    protected void ReportImpact(Vector3 point)
    {
        onImpact?.Invoke();
        CollapseBus.RaiseImpacted(this, point);
    }

    // ----- What each subclass must implement -----
    protected abstract void SaveStartState();                    // remember position, rotation, etc.
    protected abstract void RestoreStartState();                 // put everything back
    protected abstract IEnumerator WarnRoutine(float duration);  // telegraph: shake, wobble...
    protected abstract IEnumerator CollapseRoutine();            // the scripted, reliable part of the fall

    // Optional: cleanup after the collapse (hide debris, freeze rigidbodies...)
    protected virtual IEnumerator AfterCollapseRoutine() { yield break; }

    // ----- Testing helpers: right-click the component header in Play Mode -----
    [ContextMenu("TEST: Trigger this piece")]
    private void DebugTrigger() => Trigger();

    [ContextMenu("TEST: Reset everything")]
    private void DebugResetAll() => CollapseBus.ResetAll();
}
