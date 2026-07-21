using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to a UI panel (RectTransform) that acts as the joystick's draggable zone.
public class MobileJoystick : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
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

    public void OnEndDrag(PointerEventData eventData)
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

    void Start()
    {
        if (macheteButton != null) macheteButton.onClick.AddListener(() => hunter.TriggerMacheteAttackButton());
        if (musketButton != null) musketButton.onClick.AddListener(() => hunter.FireMusket());
        if (egbeButton != null) egbeButton.onClick.AddListener(() => hunter.CastEgbe());
    }

    void Update()
    {
        if (joystick != null && hunter != null)
            hunter.MobileMoveInput = joystick.InputVector;
    }
}
