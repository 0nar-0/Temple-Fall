using UnityEngine;
using System.Collections.Generic;
using System;

public class CollapseDirector : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [Tooltip("The ID of the CollapsePiece that fall together in this step.")]
        public string[] ids;

        [Tooltip("Seconds to wait before the step fires (after the previous step). 0 = use base interval.")]
        [Min(0f)] public float delay;
    }

    //------SEQUENCE------
    [Header("Sequence")]
    [Tooltip("In the order the player runs through the level")]
    [SerializeField] private List<Step> steps = new List<Step>();

    //------TIMING------
    [Header("Timing")]
    [Tooltip("Seconds between steps at default speed.")]
    [SerializeField] private float baseInterval = 2f;
    [Tooltip("Steps never fire closer together than this, even at full speed. Keep it above the pieces' Warn Duration.")]
    [SerializeField] private float minInterval = 1f;
    [Tooltip("Start on Play instead of waiting for a checkpoint with 'Starts Sequence'.")]
    [SerializeField] private bool startOnPlay = false;
    [SerializeField] private float startDelay = 0f;

    //------CATCH UP------
    [Header("Catch-up")]
    [Tooltip("Up to this many steps ahead, collapse runs at normal speed.")]
    [SerializeField] private int comfortableGap = 2;
    [Tooltip("At this many steps ahead (or more), the collapse runs at max speed")]
    [SerializeField] private int maxGap = 6;
    [SerializeField] private float maxSpeedMultiplier = 3f;

    //------DEBUG------
    [Header("Debug (DONT TOUCH, READ ONLY)")]
    [SerializeField] private bool running;
    [SerializeField] private int nextStep;
    [SerializeField] private int playerStep;
    [SerializeField] private int gap;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private float currentInterval;
    [SerializeField] private float timeUntilNextStep;

    private float timer;
    private readonly List<Vector3> stepPositions = new List<Vector3>();
    public bool IsRunning => running;
    public int StepCount => steps.Count;

    private void Awake()
    {
        ResetState();
    }

    private void OnEnable()
    {
        CollapseBus.AllReset += HandleReset;
    }
    private void OnDisable()
    {
        CollapseBus.AllReset -= HandleReset;
    }
    private void Start()
    {
        CacheStepPositions();
        if (startOnPlay)
        {
            Invoke(nameof(StartSequence), startDelay);// start after a delay, so the player can see the first step before it falls
        }
    }

    private void Update()
    {
        if (!running) return;

        if (nextStep >= steps.Count)
        {
            running = false;
            return;
        }
        gap = playerStep - nextStep;
        float interval = GetInterval(nextStep);
        multiplier = CalculateMultiplier(gap, interval);
        currentInterval = interval;
        timer += Time.deltaTime * multiplier;
        if (timer >= interval)
        {
            timer -= interval;
            FireNextStep();
        }
        if (nextStep < steps.Count)
            timeUntilNextStep = (GetInterval(nextStep) - timer) / multiplier;
        else
            timeUntilNextStep = 0f;
    }
    //------CALLED BY CHECKPOINTS------

    public void StartSequence()
    {
        if (running || nextStep >= steps.Count) return;
        running = true;
    }
    public void ReportProgress(int step)//Report the player's progress to the director.  The director will use this to determine how fast to run the collapse sequence.
    {
        playerStep = Mathf.Max(playerStep, step);
    }
    // ------INTERNAL------
    private float CalculateMultiplier(int gap, float interval)//Calculate the speed multiplier based on the gap between the player and the collapse sequence.
    {
        float t = Mathf.InverseLerp(comfortableGap, maxGap, gap);
        float m = Mathf.Lerp(1f, maxSpeedMultiplier, t);

        if (minInterval > 0f)
        {
            m = Mathf.Min(m, interval / minInterval);

        }
        return Mathf.Max(1f, m);
    }
    private float GetInterval(int index)//Get the interval for the given step index.  If the step has a delay, use that.  Otherwise, use the base interval.
    {
        float d = steps[index].delay;
        return d > 0f ? d : baseInterval;
    }

    private void FireNextStep()//Fire the next step in the sequence.  This will trigger the CollapsePieces associated with the step.
    {
        string[] ids = steps[nextStep].ids;
        if (ids != null)
        {
            foreach (string id in ids)
            {
                CollapseBus.Trigger(id);
            }
        }
        nextStep++;
    }
    private void HandleReset()
    {
        CancelInvoke();
        ResetState();
        if (startOnPlay)
        {
            Invoke(nameof(StartSequence), startDelay);
        }
    }
    private void ResetState()
    {
        running = false;
        nextStep = 0;
        playerStep = 0;
        gap = 0;
        multiplier = 1f;
        timer = 0f;
        timeUntilNextStep = 0f;
        currentInterval = 0f;
    }
    private void CacheStepPositions()
    {
        stepPositions.Clear();
        foreach (Step step in steps)
        {
            Vector3 pos = Vector3.zero;
            if (step.ids != null && step.ids.Length > 0 && CollapseBus.TryGet(step.ids[0], out CollapsePiece piece))
            {
                pos = piece.transform.position;
            }
            stepPositions.Add(pos);
        }
    }
    //------DEBUG------
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || stepPositions.Count == 0) return;

        // Red = collapse front, green = player progress.
        if (nextStep < stepPositions.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(stepPositions[nextStep] + Vector3.up, 0.5f);
        }
        if (playerStep < stepPositions.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(stepPositions[playerStep] + Vector3.up * 2f, 0.5f);
        }
    }
    [ContextMenu("TEST: Start sequence")]
    private void DebugStart() => StartSequence();
    [ContextMenu("TEST: Fire next step now")]
    private void DebugFireNext()
    {
        if (nextStep < steps.Count) FireNextStep();
    }
}
