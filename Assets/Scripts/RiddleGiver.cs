using System.Collections.Generic;
using UnityEngine;

// Attach to any spirit (Ostrich-King, a Ghommid) that should pose a riddle
// when the player gets close and presses an interact key. Wisdom is the
// resource the book's structure is built around: you bring it back to the
// village, not just loot.
//
// Two modes, chosen automatically:
//   - Pool mode: assign a RiddlePool. Each spirit draws a different
//     weighted-random riddle at Start(), and the pool tracks which riddles
//     have been used so far this playthrough so the same one doesn't turn
//     up twice at different spirits. Difficulty is also wisdom-gated: as
//     WisdomTracker.currentWisdom climbs, the effective difficulty FLOOR
//     rises (not just the ceiling), so trials measurably escalate instead
//     of staying flat across a run. Each spirit's own maxDifficulty still
//     caps how hard it's ever willing to go, regardless of wisdom.
//   - Fixed mode (unchanged from before): leave Riddle Pool empty and set
//     riddleText/correctAnswerHint/wisdomReward directly in the Inspector,
//     exactly like the original version of this script.
public class RiddleGiver : MonoBehaviour
{
    [Header("Fixed riddle (used only if no Riddle Pool is assigned below)")]
    [TextArea] public string riddleText = "What walks the forest but leaves no path?";
    [TextArea] public string correctAnswerHint = "the wind";
    public int wisdomReward = 10;

    [Header("Procedural riddle pool (optional)")]
    [Tooltip("If assigned, this spirit draws a weighted-random riddle from the pool " +
             "at Start() instead of using the fixed fields above.")]
    public RiddlePool riddlePool;
    [Tooltip("Hard ceiling — this spirit will never ask above this difficulty, no matter how much wisdom the player has.")]
    public RiddlePool.Difficulty maxDifficulty = RiddlePool.Difficulty.Hard;

    [Header("Wisdom-gated difficulty floor (optional)")]
    [Tooltip("Wisdom total at which this spirit stops offering Easy riddles.")]
    public int wisdomForMediumFloor = 30;
    [Tooltip("Wisdom total at which this spirit only offers Hard riddles (still capped by Max Difficulty above).")]
    public int wisdomForHardFloor = 80;

    public float interactRange = 4f;

    // Exposed so accessibility features (e.g. MobileTouchUI's hint toggle)
    // can check whether a spirit still has an active riddle without needing
    // their own duplicate range/resolved tracking.
    public bool IsResolved => resolved;
    public string CurrentHint => correctAnswerHint;

    private Transform player;
    private bool resolved = false;

    // Shared across every RiddleGiver drawing from the same pool asset, so a
    // single playthrough doesn't hand out the same riddle at two spirits.
    // Keyed by pool asset, so ogbojuode-3d's pool and irekeonibudo-3d's pool
    // (or two different pools in the same project) track independently.
    private static readonly Dictionary<RiddlePool, HashSet<int>> usedByPool = new Dictionary<RiddlePool, HashSet<int>>();

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (riddlePool != null && riddlePool.riddles.Count > 0)
        {
            if (!usedByPool.TryGetValue(riddlePool, out var used))
            {
                used = new HashSet<int>();
                usedByPool[riddlePool] = used;
            }

            var rng = RunSeed.NewRandom($"riddle_{gameObject.name}_{gameObject.GetInstanceID()}");
            RiddlePool.Difficulty effectiveMin = EffectiveMinDifficulty();
            int index = riddlePool.PickIndex(rng, used, maxDifficulty, effectiveMin);
            if (index >= 0)
            {
                used.Add(index);
                var entry = riddlePool.riddles[index];
                riddleText = entry.riddleText;
                correctAnswerHint = entry.correctAnswerHint;
                wisdomReward = entry.wisdomReward;
            }
            // If index is -1 (nothing eligible even after the pool's own
            // fallback-to-repeats pass), the fixed-mode fields above are
            // used as a safe fallback rather than leaving this spirit mute.
        }
    }

    // Floor rises with the player's current wisdom, capped so it never
    // exceeds this spirit's own maxDifficulty ceiling — a ghommid stays
    // capped at Medium even if wisdomForHardFloor is technically reached.
    private RiddlePool.Difficulty EffectiveMinDifficulty()
    {
        int wisdom = WisdomTracker.Instance != null ? WisdomTracker.Instance.currentWisdom : 0;

        RiddlePool.Difficulty floor = RiddlePool.Difficulty.Easy;
        if (wisdom >= wisdomForHardFloor) floor = RiddlePool.Difficulty.Hard;
        else if (wisdom >= wisdomForMediumFloor) floor = RiddlePool.Difficulty.Medium;

        return (RiddlePool.Difficulty)Mathf.Min((int)floor, (int)maxDifficulty);
    }

    void Update()
    {
        if (resolved || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= interactRange && Input.GetKeyDown(KeyCode.F))
            Interact();
    }

    // Same range/resolved checks as the F-key path above, but callable
    // directly — the keyboard's "F near a spirit" prompt has no touch
    // equivalent otherwise, so mobile players would have no way to ever
    // trigger a riddle. Wire this to an on-screen Interact button
    // (see MobileTouchUI.cs).
    public void Interact()
    {
        if (resolved || player == null) return;
        if (Vector3.Distance(transform.position, player.position) > interactRange) return;

        Debug.Log($"[{gameObject.name}] asks: \"{riddleText}\"");
        // Codex entry: recorded the moment a riddle is posed, not only when
        // solved, so the compendium reflects everything encountered even if
        // the player later fails or walks away.
        WisdomTracker.Instance?.RecordRiddleSeen(riddleText);
        // Hook your real dialogue/riddle UI here. This stub auto-resolves
        // as correct so the loop is testable without a UI yet.
        ResolveRiddle(true);
    }

    public void ResolveRiddle(bool answeredCorrectly)
    {
        resolved = true;

        if (answeredCorrectly)
        {
            WisdomTracker.Instance?.AddWisdom(wisdomReward);
            Debug.Log($"[{gameObject.name}] The spirit nods. +{wisdomReward} wisdom.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] The spirit's patience ends.");
            // SendMessage rather than a direct type reference so this stays
            // usable by either OstrichKingBoss or ArogidigbaBoss unmodified.
            SendMessage("OnRiddleFailed", SendMessageOptions.DontRequireReceiver);
        }
    }

    // Call at the start of a new playthrough (e.g. from a "New Game" menu)
    // to clear which riddles have already been shown, so a fresh run isn't
    // stuck avoiding riddles seen in a previous one.
    public static void ResetPoolMemory() => usedByPool.Clear();
}
