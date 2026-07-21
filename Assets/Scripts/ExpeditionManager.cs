using UnityEngine;

// Mirrors the book's structure: expeditions launch from the safe hub into
// the forest and return. This doesn't gate anything by itself (no lockouts) —
// it just tracks state and logs transitions, so you can hook UI, music
// changes, or a "expedition complete" summary onto it later.
public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance { get; private set; }

    public Transform player;
    public float villageBoundaryZ = 15f; // matches the spike-line Z in SceneSetupWizard

    public bool InForest { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (player == null) return;

        bool nowInForest = player.position.z > villageBoundaryZ;
        if (nowInForest != InForest)
        {
            InForest = nowInForest;
            if (InForest)
                Debug.Log("Expedition begins. The village falls behind.");
            else
                Debug.Log($"Returned to the hub. Wisdom carried: {(WisdomTracker.Instance != null ? WisdomTracker.Instance.currentWisdom : 0)}");
        }
    }
}
