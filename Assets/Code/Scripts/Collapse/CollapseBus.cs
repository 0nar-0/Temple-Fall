using System;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  SHARED CONTRACT between Game AI and Physics & Collisions.
//
//  Game AI decides WHEN something collapses  -> calls CollapseBus.Trigger("ID")
//  Physics decides HOW it collapses           -> CollapsePiece subclasses
//  Audio / VFX / AI can LISTEN                -> subscribe to the events below
//
//  Change this file only after both of you agree on it.
// =====================================================================
public static class CollapseBus
{
    private static readonly Dictionary<string, CollapsePiece> pieces = new Dictionary<string, CollapsePiece>();

    // --- Events anyone can listen to (Audio, VFX, AI, camera shake...) ---
    public static event Action<CollapsePiece> PieceWarned;               // telegraph started (shake, dust, warning sound)
    public static event Action<CollapsePiece> PieceCollapsed;            // the real collapse started
    public static event Action<CollapsePiece, Vector3> PieceImpacted;    // piece hit the ground / something (position of impact)
    public static event Action AllReset;                                 // level restarted, everything back to start

    // --- Commands (mainly used by Game AI) ---

    /// Warn first (telegraph), then collapse. The normal way to start a collapse.
    public static bool Trigger(string id)
    {
        if (!TryGet(id, out CollapsePiece piece)) return false;
        piece.Trigger();
        return true;
    }

    /// Collapse immediately, skipping the warning. Use rarely, it feels unfair to the player.
    public static bool CollapseNow(string id)
    {
        if (!TryGet(id, out CollapsePiece piece)) return false;
        piece.CollapseNow();
        return true;
    }

    /// Put every piece back to its start state. Call this when the player dies and restarts.
    public static void ResetAll()
    {
        foreach (CollapsePiece piece in pieces.Values)
            if (piece != null) piece.ResetPiece();
        AllReset?.Invoke();
    }

    public static bool TryGet(string id, out CollapsePiece piece)
    {
        if (id != null && pieces.TryGetValue(id, out piece) && piece != null) return true;
        Debug.LogWarning($"CollapseBus: no piece with ID '{id}'. Check the spelling in the inspector.");
        piece = null;
        return false;
    }

    // --- Used internally by CollapsePiece ---

    internal static void Register(CollapsePiece piece)
    {
        if (string.IsNullOrEmpty(piece.Id))
        {
            Debug.LogWarning($"CollapseBus: '{piece.name}' has no Collapse ID, AI can't trigger it.", piece);
            return;
        }
        if (pieces.TryGetValue(piece.Id, out CollapsePiece existing) && existing != null && existing != piece)
            Debug.LogError($"CollapseBus: duplicate ID '{piece.Id}' on '{piece.name}' and '{existing.name}'.", piece);
        pieces[piece.Id] = piece;
    }

    internal static void Unregister(CollapsePiece piece)
    {
        if (piece.Id != null && pieces.TryGetValue(piece.Id, out CollapsePiece existing) && existing == piece)
            pieces.Remove(piece.Id);
    }

    internal static void RaiseWarned(CollapsePiece p) => PieceWarned?.Invoke(p);
    internal static void RaiseCollapsed(CollapsePiece p) => PieceCollapsed?.Invoke(p);
    internal static void RaiseImpacted(CollapsePiece p, Vector3 point) => PieceImpacted?.Invoke(p, point);

    // Clears static data when entering Play Mode (needed if "Enter Play Mode Options" skips domain reload).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearStatics()
    {
        pieces.Clear();
        PieceWarned = null;
        PieceCollapsed = null;
        PieceImpacted = null;
        AllReset = null;
    }
}
