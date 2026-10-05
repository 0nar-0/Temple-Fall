using UnityEngine;
[RequireComponent(typeof(Collider))]
public class CollapseCheckpoint : MonoBehaviour
{


    [SerializeField] private CollapseDirector director;

    [Tooltip("Index in the director's step list that lines up with this spot (0 = first step).")]
    [SerializeField] private int stepIndex;

    [Tooltip("Entering this checkpoint starts the timed collapse.")]
    [SerializeField] private bool startsSequence;

    [SerializeField] private string playerTag = "Player";

    public int StepIndex => stepIndex;

    private void Awake()
    {
        if (director == null) director = FindFirstObjectByType<CollapseDirector>();// auto-find the director if not set
        GetComponent<Collider>().isTrigger = true;// make sure the collider is a trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController p)) return;

        if (director != null)
        {
            director.ReportProgress(stepIndex);
            if (startsSequence) director.StartSequence();
        }
    }

    //------DEBUGGING------
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        if (TryGetComponent(out BoxCollider box))
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, $"Step {stepIndex}");
#endif
    }

}
