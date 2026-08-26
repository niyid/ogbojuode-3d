using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A bank of riddles RiddleGiver can draw from at runtime instead of using
// one hardcoded riddle per spirit. Create one via
// Assets > Create > Yoruba3D > Riddle Pool, fill in entries in the
// Inspector, and drag it onto any RiddleGiver's "Riddle Pool" field.
//
// Story-agnostic — the same script works for ogbojuode-3d's forest riddles
// and irekeonibudo-3d's undersea riddles; only the pool *asset* content
// differs per project.
[CreateAssetMenu(fileName = "RiddlePool", menuName = "Yoruba3D/Riddle Pool")]
public class RiddlePool : ScriptableObject
{
    public enum Difficulty { Easy, Medium, Hard }

    [System.Serializable]
    public class RiddleEntry
    {
        [TextArea] public string riddleText;
        [TextArea] public string correctAnswerHint;
        public int wisdomReward = 10;
        public Difficulty difficulty = Difficulty.Easy;
        [Tooltip("Higher weight = more likely to be picked when eligible. 1 = normal.")]
        public float weight = 1f;
    }

    public List<RiddleEntry> riddles = new List<RiddleEntry>();

    // Weighted-random pick within [minDifficulty, maxDifficulty], excluding
    // indices already used this playthrough. minDifficulty defaults to Easy
    // so existing callers behave exactly as before; RiddleGiver passes a
    // wisdom-derived floor to make trials escalate as the player progresses.
    // If nothing eligible in that band, first widens back down to Easy at
    // the same maxDifficulty (so a player who's raced ahead on wisdom isn't
    // just given nothing); if STILL nothing, clears the exclusion set and
    // retries — repeats become possible again rather than the game breaking.
    public int PickIndex(System.Random rng, HashSet<int> excludeIndices, Difficulty maxDifficulty,
        Difficulty minDifficulty = Difficulty.Easy)
    {
        var eligible = Eligible(excludeIndices, minDifficulty, maxDifficulty);

        if (eligible.Count == 0 && minDifficulty != Difficulty.Easy)
            eligible = Eligible(excludeIndices, Difficulty.Easy, maxDifficulty);

        if (eligible.Count == 0)
        {
            excludeIndices.Clear();
            eligible = Eligible(excludeIndices, Difficulty.Easy, maxDifficulty);
        }

        if (eligible.Count == 0) return -1; // pool empty, or nothing at/under maxDifficulty

        float totalWeight = eligible.Sum(i => Mathf.Max(0.01f, riddles[i].weight));
        double roll = rng.NextDouble() * totalWeight;
        double cumulative = 0;
        foreach (int i in eligible)
        {
            cumulative += Mathf.Max(0.01f, riddles[i].weight);
            if (roll <= cumulative) return i;
        }
        return eligible[eligible.Count - 1];
    }

    private List<int> Eligible(HashSet<int> excludeIndices, Difficulty minDifficulty, Difficulty maxDifficulty)
    {
        return Enumerable.Range(0, riddles.Count)
            .Where(i => riddles[i].difficulty >= minDifficulty
                     && riddles[i].difficulty <= maxDifficulty
                     && !excludeIndices.Contains(i))
            .ToList();
    }
}
