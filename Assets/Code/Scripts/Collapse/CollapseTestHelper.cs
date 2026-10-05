using UnityEngine;
using UnityEngine.InputSystem;

// =====================================================================
//  OWNER: Physics & Collisions  -  TESTING ONLY, keep it out of Main.unity
//
//  Put this on any empty GameObject in a test scene. It:
//   - prints every CollapseBus signal to the Console (proves the AI -> Physics
//     -> Audio chain works before Audio exists)
//   - draws a red sphere where pieces hit something (impact positions)
//   - R = reset everything and teleport the player back to the start
//   - 1..9 = trigger the pieces listed in "Hotkey IDs" (test without triggers)
// =====================================================================
public class CollapseTestHelper : MonoBehaviour
{
    [Tooltip("Optional: the player, so R puts them back at the start.")]
    [SerializeField] private Transform player;

    [Tooltip("Keys 1-9 trigger these IDs in order.")]
    [SerializeField] private string[] hotkeyIds;

    private Vector3 playerStartPos;
    private Quaternion playerStartRot;
    private Vector3 lastImpact;
    private float lastImpactTime = -10f;

    private void Awake()
    {
        if (player != null)
        {
            playerStartPos = player.position;
            playerStartRot = player.rotation;
        }
    }

    private void OnEnable()
    {
        CollapseBus.PieceWarned += OnWarned;
        CollapseBus.PieceCollapsed += OnCollapsed;
        CollapseBus.PieceImpacted += OnImpacted;
        CollapseBus.AllReset += OnReset;
    }

    private void OnDisable()
    {
        CollapseBus.PieceWarned -= OnWarned;
        CollapseBus.PieceCollapsed -= OnCollapsed;
        CollapseBus.PieceImpacted -= OnImpacted;
        CollapseBus.AllReset -= OnReset;
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame)
        {
            CollapseBus.ResetAll();
            ResetPlayer();
        }

        if (hotkeyIds == null) return;
        for (int i = 0; i < hotkeyIds.Length && i < 9; i++)
        {
            Key key = Key.Digit1 + i;
            if (kb[key].wasPressedThisFrame)
                CollapseBus.Trigger(hotkeyIds[i]);
        }
    }

    private void ResetPlayer()
    {
        if (player == null) return;
        // A CharacterController overrides position changes while enabled,
        // so switch it off for the teleport.
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetPositionAndRotation(playerStartPos, playerStartRot);
        if (cc != null) cc.enabled = true;
    }

    private void OnWarned(CollapsePiece p) =>
        Debug.Log($"<color=yellow>[WARN]</color> {p.Id}  ({Time.time:F2}s)", p);

    private void OnCollapsed(CollapsePiece p) =>
        Debug.Log($"<color=orange>[COLLAPSE]</color> {p.Id}  ({Time.time:F2}s)", p);

    private void OnImpacted(CollapsePiece p, Vector3 point)
    {
        Debug.Log($"<color=red>[IMPACT]</color> {p.Id} at {point}  ({Time.time:F2}s)", p);
        lastImpact = point;
        lastImpactTime = Time.time;
    }

    private void OnReset() => Debug.Log("<color=cyan>[RESET ALL]</color>");

    private void OnDrawGizmos()
    {
        if (Time.time - lastImpactTime > 2f) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastImpact, 0.5f);
    }
}
