using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A branching pool of ambient lines for non-riddle dialogue (a ghommid's
// muttering as you pass, the mother-spirit-guide's asides) — not the riddle
// itself, which stays on RiddleGiver/RiddlePool. Each line can point at a
// set of possible follow-up lines; DialogueSpeaker walks the tree, picking
// weighted-random branches, so repeat encounters don't play back verbatim.
//
// Story-agnostic — identical script for both projects; only the tree
// *asset* content (which lines, which branches) differs per story.
[CreateAssetMenu(fileName = "DialogueTree", menuName = "Yoruba3D/Dialogue Tree")]
public class DialogueTree : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string id;
        [TextArea] public string text;
        [Tooltip("Higher weight = more likely to be picked among siblings.")]
        public float weight = 1f;
        [Tooltip("IDs of lines that can follow this one. Leave empty to end this branch " +
                 "(the next Speak() call restarts from an opening line).")]
        public List<string> nextLineIds = new List<string>();
    }

    [Tooltip("IDs of lines eligible to open a conversation.")]
    public List<string> openingLineIds = new List<string>();
    public List<Line> lines = new List<Line>();

    private Dictionary<string, Line> _byId;
    private Dictionary<string, Line> ById
    {
        get
        {
            if (_byId == null) _byId = lines.ToDictionary(l => l.id, l => l);
            return _byId;
        }
    }

    public Line PickOpening(System.Random rng) => PickWeighted(rng, openingLineIds);

    public Line PickNext(System.Random rng, Line current) =>
        (current == null || current.nextLineIds.Count == 0) ? null : PickWeighted(rng, current.nextLineIds);

    private Line PickWeighted(System.Random rng, List<string> candidateIds)
    {
        var candidates = candidateIds.Where(ById.ContainsKey).Select(id => ById[id]).ToList();
        if (candidates.Count == 0) return null;

        float total = candidates.Sum(l => Mathf.Max(0.01f, l.weight));
        double roll = rng.NextDouble() * total;
        double cumulative = 0;
        foreach (var l in candidates)
        {
            cumulative += Mathf.Max(0.01f, l.weight);
            if (roll <= cumulative) return l;
        }
        return candidates[candidates.Count - 1];
    }
}
