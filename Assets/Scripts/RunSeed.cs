using UnityEngine;

// One seed per playthrough. Every procedural system (riddle selection,
// creature stat variance, spirit placement, dialogue branching) derives its
// randomness from this seed via a named stream, so:
//   - the same seed always reproduces the same "shape" of world (useful for
//     debugging, seed-sharing, or a daily-challenge mode later)
//   - different systems don't draw from the same sequence and end up
//     correlated with each other (e.g. riddle order affecting spirit
//     placement)
//   - a fresh playthrough (no forced seed) still gets real variety
//
// Story-agnostic — identical in ogbojuode-3d and irekeonibudo-3d.
public static class RunSeed
{
    public static int Value { get; private set; }
    private static bool initialized = false;

    // Call explicitly at the start of a run (e.g. from a "New Game" button)
    // if you want a specific/shareable seed. If nothing calls this, the
    // first NewRandom() call auto-initializes with a random seed.
    public static void Initialize(int? forcedSeed = null)
    {
        Value = forcedSeed ?? System.Guid.NewGuid().GetHashCode();
        initialized = true;
        Debug.Log($"[RunSeed] Playthrough seed: {Value}");
    }

    public static System.Random NewRandom(string streamName)
    {
        if (!initialized) Initialize();
        int combined = Value ^ streamName.GetHashCode();
        return new System.Random(combined);
    }
}
