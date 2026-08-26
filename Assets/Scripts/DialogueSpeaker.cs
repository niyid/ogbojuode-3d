using UnityEngine;

// Attach alongside GhommidSpirit (or MotherSpiritGuide in irekeonibudo-3d)
// to have it speak from a DialogueTree instead of one fixed line. Auto-fires
// when the player first comes within range (like RiddleGiver's interact
// range, but passive — no key press needed, since this is ambient flavor
// rather than the riddle interaction itself). You can also call Speak()
// manually from other triggers if you want it tied to something else.
//
// Same "auto-resolves without a UI yet" pattern as RiddleGiver: this logs to
// the console for now. Wire a real dialogue box onto the OnLineSpoken
// callback when you're ready.
public class DialogueSpeaker : MonoBehaviour
{
    public DialogueTree tree;
    public float triggerRange = 6f;
    [Tooltip("Minimum seconds between auto-triggered lines from this speaker.")]
    public float retriggerCooldown = 8f;

    public System.Action<string> OnLineSpoken;

    private System.Random rng;
    private DialogueTree.Line current;
    private Transform player;
    private bool playerWasInRange = false;
    private float nextAllowedTime = 0f;

    void Start()
    {
        rng = RunSeed.NewRandom($"dialogue_{gameObject.GetInstanceID()}");
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || tree == null) return;

        bool inRange = Vector3.Distance(transform.position, player.position) <= triggerRange;
        if (inRange && !playerWasInRange && Time.time >= nextAllowedTime)
        {
            Speak();
            nextAllowedTime = Time.time + retriggerCooldown;
        }
        playerWasInRange = inRange;
    }

    public void Speak()
    {
        if (tree == null) return;

        current = current == null
            ? tree.PickOpening(rng)
            : (tree.PickNext(rng, current) ?? tree.PickOpening(rng));

        if (current == null) return;

        Debug.Log($"[{gameObject.name}] {current.text}");
        OnLineSpoken?.Invoke(current.text);
    }
}
