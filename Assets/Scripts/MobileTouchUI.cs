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

    void Start()
    {
        if (macheteButton != null) macheteButton.onClick.AddListener(() => hunter.TriggerMacheteAttackButton());
        if (musketButton != null) musketButton.onClick.AddListener(() => hunter.FireMusket());
        if (egbeButton != null) egbeButton.onClick.AddListener(() => hunter.CastEgbe());
        if (interactButton != null) interactButton.onClick.AddListener(TryInteractWithNearbyRiddleGiver);
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
    }
}
