using UnityEngine;

// =====================================================================
//  OWNER: Physics & Collisions
//
//  A piece that can be broken by being HIT (by a falling pillar, debris...)
//  instead of by the AI. Smashing skips the warning and the scripted part,
//  and goes straight to real physics, pushed in the direction of the hit.
// =====================================================================
public interface ISmashable
{
    /// point = where it was hit, velocityChange = push given to the piece (m/s)
    void Smash(Vector3 point, Vector3 velocityChange);
}
