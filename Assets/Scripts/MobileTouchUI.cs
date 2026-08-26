using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to a UI panel (RectTransform) that acts as the joystick's draggable zone.
public class MobileJoystick : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform knob;
    public RectTransform background;
    public Vector2 InputVector { get; private set; }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            float radius = background.sizeDelta.x / 2f;
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
            knob.anchoredPosition = clamped;
            InputVector = clamped / radius;
        }
    }

    public void OnEndDrag(PointerEventData eventData) => ResetJoystick();

    // A tap-and-release that never crosses Unity's drag threshold fires
    // OnPointerDown but never OnEndDrag, which left InputVector stuck
    // non-zero — the player would keep walking after lifting their thumb.
    // IPointerUpHandler catches every release, dragged or not.
    public void OnPointerUp(PointerEventData eventData) => ResetJoystick();

    private void ResetJoystick()
    {
        InputVector = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
    }
}

// Bridges the joystick + buttons to the player controller. Wire up references in the Inspector.
public class MobileTouchUI : MonoBehaviour
{
    public YorubaHunterController hunter;
    public MobileJoystick joystick;
    public Button macheteButton;
    public Button musketButton;
    public Button egbeButton;
    public Button interactButton;

    [Header("Accessibility (optional)")]
    [Tooltip("Toggle for players who want riddle hints surfaced automatically. Persists across sessions.")]
    public Toggle hintToggle;
    [Tooltip("Text element the hint is written to when Hint Toggle is on. Leave unset to only log to console.")]
    public Text hintDisplay;
    public float hintTriggerRange = 4f;

    private const string HintsEnabledPrefsKey = "accessibility_hintsEnabled";
    private Transform player;

    void Start()
    {
        if (macheteButton != null) macheteButton.onClick.AddListener(() => hunter.TriggerMacheteAttackButton());
        if (musketButton != null) musketButton.onClick.AddListener(() => hunter.FireMusket());
        if (egbeButton != null) egbeButton.onClick.AddListener(() => hunter.CastEgbe());
        if (interactButton != null) interactButton.onClick.AddListener(TryInteractWithNearbyRiddleGiver);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (hintToggle != null)
        {
            hintToggle.isOn = PlayerPrefs.GetInt(HintsEnabledPrefsKey, 0) == 1;
            hintToggle.onValueChanged.AddListener(OnHintToggleChanged);
        }
    }

    private void OnHintToggleChanged(bool enabled)
    {
        PlayerPrefs.SetInt(HintsEnabledPrefsKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (!enabled && hintDisplay != null) hintDisplay.text = "";
    }

    // Keyboard players interact via "F near a spirit"; touch has no
    // equivalent key, so this hits every RiddleGiver in the scene and lets
    // each one's own range/resolved check decide whether it responds.
    private void TryInteractWithNearbyRiddleGiver()
    {
        RiddleGiver[] riddleGivers = FindObjectsOfType<RiddleGiver>();
        foreach (RiddleGiver giver in riddleGivers)
            giver.Interact();
    }

    void Update()
    {
        if (joystick != null && hunter != null)
            hunter.MobileMoveInput = joystick.InputVector;

        if (hintToggle != null && hintToggle.isOn)
            UpdateNearestHint();
    }

    // Surfaces the correctAnswerHint of the nearest unresolved riddle spirit
    // while the accessibility toggle is on. Doesn't change wisdomReward or
    // pool behavior — purely a presentation aid for players who want it,
    // opt-in and off by default.
    private void UpdateNearestHint()
    {
        if (player == null || hintDisplay == null) return;

        RiddleGiver nearest = null;
        float nearestDist = hintTriggerRange;
        foreach (RiddleGiver giver in FindObjectsOfType<RiddleGiver>())
        {
            if (giver.IsResolved) continue;
            float dist = Vector3.Distance(giver.transform.position, player.position);
            if (dist <= nearestDist)
            {
                nearestDist = dist;
                nearest = giver;
            }
        }

        hintDisplay.text = nearest != null ? $"Hint: {nearest.CurrentHint}" : "";
    }
}
