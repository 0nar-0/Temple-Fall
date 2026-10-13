using UnityEngine;

public class SwingPlatfrom : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private Vector3 swingAxis = Vector3.forward;
    [SerializeField, Range(0f, 180f)] private float swingAngle = 45f;
    [SerializeField, Min(0.01f)] private float cycleDuration = 3f;

    private Quaternion startingRotation;
    private float elapsedTime;

    private void Awake()
    {
        if (pivot == null)
            pivot = transform;

        startingRotation = pivot.localRotation;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        float angle = Mathf.Sin(
            elapsedTime * Mathf.PI * 2f / cycleDuration
        ) * swingAngle;

        if (swingAxis.sqrMagnitude > 0f)
        {
            pivot.localRotation = startingRotation *
                Quaternion.AngleAxis(angle, swingAxis.normalized);
        }
    }
}
