using UnityEngine;

// The "bring wisdom back to civilization" resource from the book's core
// loop. Deliberately separate from PlayerVitals — wisdom persists even if
// you die and respawn at the hub, health doesn't.
public class WisdomTracker : MonoBehaviour
{
    public static WisdomTracker Instance { get; private set; }

    public int currentWisdom = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddWisdom(int amount)
    {
        currentWisdom += amount;
        Debug.Log($"Wisdom carried: {currentWisdom}");
    }
}
