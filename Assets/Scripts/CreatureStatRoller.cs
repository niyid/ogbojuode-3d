using UnityEngine;

// Adds run-to-run variance on top of CreatureAI's fixed presets so the same
// creature type isn't byte-identical in every playthrough. Call this right
// after CreatureAI.ApplyPreset(ai, type) — it nudges the already-applied
// stats rather than replacing the preset system, so the tuned baseline
// (Agbako tankier than Eru, etc.) is always preserved; only the exact
// numbers shift.
//
// Variance is intentionally modest so a run never feels unfair — a slightly
// faster Eru or slightly tankier Agbako reads as "this one's rougher," not
// "this fight is broken."
//
// Story-agnostic — identical for ogbojuode-3d and irekeonibudo-3d, since it
// only touches CreatureAI's numeric fields, not creature-specific logic.
public static class CreatureStatRoller
{
    private const float variance = 0.15f; // +/- 15%

    public static void RollVariance(CreatureAI ai)
    {
        var rng = RunSeed.NewRandom($"creature_{ai.gameObject.GetInstanceID()}");
        ai.maxHealth = RollInt(rng, ai.maxHealth);
        ai.chaseSpeed = RollFloat(rng, ai.chaseSpeed);
        ai.contactDamage = RollInt(rng, ai.contactDamage);
        ai.aggroRange = RollFloat(rng, ai.aggroRange);
    }

    private static int RollInt(System.Random rng, int baseValue)
    {
        float factor = 1f + (float)(rng.NextDouble() * 2 - 1) * variance;
        return Mathf.Max(1, Mathf.RoundToInt(baseValue * factor));
    }

    private static float RollFloat(System.Random rng, float baseValue)
    {
        float factor = 1f + (float)(rng.NextDouble() * 2 - 1) * variance;
        return baseValue * factor;
    }
}
