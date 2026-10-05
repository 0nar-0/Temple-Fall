# Collapse system: Game AI ↔ Physics contract

**Rule:** AI decides **WHEN**. Physics decides **HOW**. Audio and VFX **LISTEN**.

```
 Game AI                         CollapseBus                     Physics
 ───────                         ───────────                     ───────
 CollapseSequenceTrigger  ──▶  Trigger("Floor_A")  ──▶  FallingPlatform (ID = "Floor_A")
 (or any AI script)                                       Idle → Warning → Collapsing → Collapsed
                                     │
                                     ▼  events
                    PieceWarned / PieceCollapsed / PieceImpacted / AllReset
                    → Audio (warning + impact sounds), VFX (dust), camera shake
```

## Files and owners

| File | Owner | Notes |
|---|---|---|
| `CollapseBus.cs` | **Shared** | Only change it after both of you agree. |
| `CollapsePiece.cs` | **Shared** | Base class. Only change it after both of you agree. |
| `FallingPlatform.cs` (and future pillars, walls…) | Physics | |
| `CollapseSequenceTrigger.cs` | Game AI | A starting example. AI can rewrite it or add a director. |

## How AI triggers things

```csharp
CollapseBus.Trigger("Pillar_01");     // warn, then collapse (normal)
CollapseBus.CollapseNow("Pillar_01"); // no warning (use rarely)
CollapseBus.ResetAll();               // on player death / restart
```

## How Audio listens

```csharp
CollapseBus.PieceWarned    += piece => PlayRumble(piece.transform.position);
CollapseBus.PieceImpacted  += (piece, point) => PlayCrash(point);
```

You can also drag things into the `onWarn`, `onCollapse` and `onImpact` UnityEvents on each piece in the inspector.

## Naming IDs

Use `Area_Type_Number`, for example `Hall_Floor_01` or `Bridge_Pillar_02`. IDs must be unique. The console warns about typos and duplicates.

## Layers (Project Settings → Tags and Layers)

Create these layers: `Player`, `Environment`, `Hazard`, `Debris`.

In Physics → Layer Collision Matrix, untick **Debris × Player**, so flying rubble never blocks or traps the player.

## Testing

Enter Play Mode, then right-click the header of a piece's component. Pick **TEST: Trigger this piece** or **TEST: Reset everything**.
