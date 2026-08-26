using System.Collections.Generic;
using UnityEngine;

// Scatters spirit/ghommid spawn points inside a rectangular band instead of
// using fixed hand-placed coordinates, so their locations differ each
// playthrough while staying clear of creature spawns and of each other.
// Used by SceneSetupWizard.BuildGhommids in place of a fixed Vector3[].
//
// Story-agnostic — identical for both projects; only the bounds and
// avoidPoints passed in differ (forest band vs. undersea-kingdom band).
public static class ProceduralSpiritPlacer
{
    public static List<Vector3> GeneratePositions(
        int count,
        float minZ, float maxZ, float minX, float maxX,
        List<Vector3> avoidPoints, float minClearance,
        string seedStream)
    {
        var rng = RunSeed.NewRandom(seedStream);
        var results = new List<Vector3>();
        int attempts = 0;
        int maxAttempts = count * 50;

        while (results.Count < count && attempts < maxAttempts)
        {
            attempts++;
            Vector3 candidate = new Vector3(Lerp(rng, minX, maxX), 0f, Lerp(rng, minZ, maxZ));

            bool tooClose = false;
            foreach (var p in avoidPoints)
                if (Vector3.Distance(candidate, p) < minClearance) { tooClose = true; break; }
            if (!tooClose)
                foreach (var p in results)
                    if (Vector3.Distance(candidate, p) < minClearance) { tooClose = true; break; }

            if (!tooClose) results.Add(candidate);
        }

        // If attempts ran out before hitting `count`, we return fewer,
        // well-spaced spirits rather than silently letting two overlap.
        if (results.Count < count)
            Debug.LogWarning($"[ProceduralSpiritPlacer] Only placed {results.Count}/{count} " +
                              "spirits without violating clearance — area may be too small/crowded.");

        return results;
    }

    private static float Lerp(System.Random rng, float min, float max) =>
        min + (float)rng.NextDouble() * (max - min);

    // Small positional variance around a hand-placed base position — used
    // for creatures, which keep their authored staging (Eru near the start,
    // Agbako at the end) but shouldn't stand in the exact same spot every
    // playthrough. Deliberately not routed through the full scatter-and-
    // avoid logic above: a fixed staging order matters for creatures in a
    // way it doesn't for wandering spirits.
    public static Vector3 JitterPosition(Vector3 basePosition, float radius, string seedStream)
    {
        var rng = RunSeed.NewRandom(seedStream);
        Vector2 offset = new Vector2(
            (float)(rng.NextDouble() * 2 - 1),
            (float)(rng.NextDouble() * 2 - 1)) * radius;
        return basePosition + new Vector3(offset.x, 0f, offset.y);
    }
}
