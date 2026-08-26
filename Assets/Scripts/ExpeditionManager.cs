using UnityEngine;

// Mirrors the book's structure: expeditions launch from the safe hub into
// the forest and return. This doesn't gate anything by itself (no lockouts) —
// it just tracks state and logs transitions, so you can hook UI, music
// changes, or a "expedition complete" summary onto it later.
public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

    public Transform player;

    // Replaces the old standalone villageBoundaryZ float — SceneSetupWizard
    // now assigns this once from WorldBounds.OgbojuOdeDefaults, and the same
    // struct is what BuildCreatures/BuildGhommids use for spawn placement,
    // so the boundary and the spawn band can't silently drift apart.
    public WorldBounds bounds = WorldBounds.OgbojuOdeDefaults;

    public bool InForest { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (player == null) return;

        bool nowInForest = player.position.z > bounds.villageBoundaryZ;
        if (nowInForest != InForest)
        {
            InForest = nowInForest;
            if (InForest)
                Debug.Log("Expedition begins. The village falls behind.");
            else
            {
                Debug.Log($"Returned to the hub. Wisdom carried: {(WisdomTracker.Instance != null ? WisdomTracker.Instance.currentWisdom : 0)}");
                // Natural checkpoint: save whenever the player makes it back
                // to safety, rather than requiring an explicit save menu yet.
                WisdomTracker.Instance?.Save();
            }
        }
    }
}
