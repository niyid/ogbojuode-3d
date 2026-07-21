using UnityEngine;

// Attach to any spirit (Ostrich-King, a Ghommid) that should pose a riddle
// when the player gets close and presses an interact key. Wisdom is the
// resource the book's structure is built around: you bring it back to the
// village, not just loot.
public class RiddleGiver : MonoBehaviour
{
    [TextArea] public string riddleText = "What walks the forest but leaves no path?";
    [TextArea] public string correctAnswerHint = "the wind";
    public int wisdomReward = 10;
    public float interactRange = 4f;

    private Transform player;
    private bool resolved = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
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
            OstrichKingBoss boss = GetComponent<OstrichKingBoss>();
            if (boss != null) boss.OnRiddleFailed();
        }
    }
}
