using System.Collections.Generic;
using UnityEngine;

// The "bring wisdom back to civilization" resource from the book's core
// loop. Deliberately separate from PlayerVitals — wisdom persists even if
// you die and respawn at the hub, health doesn't.
//
// Also owns the codex/compendium: which riddles and creatures this save has
// already encountered. RiddleGiver/RiddlePool only prevent repeats *within*
// one playthrough (their exclusion sets live in memory and reset on
// restart); this HashSet is what makes that anti-repeat survive across
// sessions, and doubles as data for a future "journal" UI.
public class WisdomTracker : MonoBehaviour
{
    public static WisdomTracker Instance { get; private set; }

    public int currentWisdom = 0;

    // Riddle text is used as the codex key rather than a numeric pool index,
    // since pool contents/order can change between builds but the riddle's
    // own wording is stable — safer to persist across app updates.
    private readonly HashSet<string> seenRiddles = new HashSet<string>();
    private readonly HashSet<string> seenCreatureTypes = new HashSet<string>();

    private const string PrefsKeyWisdom = "wisdom_currentWisdom";
    private const string PrefsKeyRiddles = "wisdom_seenRiddles";     // pipe-delimited
    private const string PrefsKeyCreatures = "wisdom_seenCreatures"; // pipe-delimited
    private const char Delimiter = '|';

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load(); // picks up currentWisdom=0 / empty codex on a fresh install, same as before
    }

    public void AddWisdom(int amount)
    {
        currentWisdom += amount;
        Debug.Log($"Wisdom carried: {currentWisdom}");
    }

    // Call from RiddleGiver when a riddle is first posed (not just when
    // solved), so the codex reflects everything the player has been asked,
    // and RiddlePool can use HasSeenRiddle to bias away from repeats even
    // across sessions once persistence is loaded.
    public void RecordRiddleSeen(string riddleText)
    {
        if (!string.IsNullOrEmpty(riddleText)) seenRiddles.Add(riddleText);
    }

    public bool HasSeenRiddle(string riddleText) => seenRiddles.Contains(riddleText);

    public void RecordCreatureSeen(string creatureTypeName)
    {
        if (!string.IsNullOrEmpty(creatureTypeName)) seenCreatureTypes.Add(creatureTypeName);
    }

    public IReadOnlyCollection<string> SeenRiddles => seenRiddles;
    public IReadOnlyCollection<string> SeenCreatureTypes => seenCreatureTypes;

    // Simple PlayerPrefs persistence — enough to make cross-session
    // anti-repeat and a wisdom total survive an app restart without pulling
    // in a full save-file format yet. Swap the backing store later without
    // changing this class's public surface.
    public void Save()
    {
        PlayerPrefs.SetInt(PrefsKeyWisdom, currentWisdom);
        PlayerPrefs.SetString(PrefsKeyRiddles, string.Join(Delimiter.ToString(), seenRiddles));
        PlayerPrefs.SetString(PrefsKeyCreatures, string.Join(Delimiter.ToString(), seenCreatureTypes));
        PlayerPrefs.Save();
        Debug.Log($"[WisdomTracker] Saved: {currentWisdom} wisdom, {seenRiddles.Count} riddles seen.");
    }

    public void Load()
    {
        currentWisdom = PlayerPrefs.GetInt(PrefsKeyWisdom, 0);

        seenRiddles.Clear();
        string riddleData = PlayerPrefs.GetString(PrefsKeyRiddles, "");
        foreach (string r in riddleData.Split(Delimiter))
            if (!string.IsNullOrEmpty(r)) seenRiddles.Add(r);

        seenCreatureTypes.Clear();
        string creatureData = PlayerPrefs.GetString(PrefsKeyCreatures, "");
        foreach (string c in creatureData.Split(Delimiter))
            if (!string.IsNullOrEmpty(c)) seenCreatureTypes.Add(c);

        Debug.Log($"[WisdomTracker] Loaded: {currentWisdom} wisdom, {seenRiddles.Count} riddles previously seen.");
    }

    // Wipes both the live session state and the persisted PlayerPrefs data —
    // for a "New Game" option distinct from just relaunching the app.
    public void ResetAll()
    {
        currentWisdom = 0;
        seenRiddles.Clear();
        seenCreatureTypes.Clear();
        PlayerPrefs.DeleteKey(PrefsKeyWisdom);
        PlayerPrefs.DeleteKey(PrefsKeyRiddles);
        PlayerPrefs.DeleteKey(PrefsKeyCreatures);
        RiddleGiver.ResetPoolMemory();
    }
}
