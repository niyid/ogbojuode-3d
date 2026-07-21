using UnityEngine;

// Small forest gnome-spirit (Elere/Ghommid): not a combat encounter. Wanders
// a small radius, sits still relative to the player, and — like the
// Ostrich-King — carries a RiddleGiver rather than a health bar. Mischievous
// per the source material, so it drifts instead of holding position.
[RequireComponent(typeof(RiddleGiver))]
public class GhommidSpirit : MonoBehaviour
{
    public float wanderRadius = 5f;
    public float wanderSpeed = 1.2f;

    private Vector3 homePosition;
    private Vector3 wanderTarget;

    void Start()
    {
        homePosition = transform.position;
        PickNewWanderTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f)
            PickNewWanderTarget();
    }

    private void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = homePosition + new Vector3(offset.x, 0f, offset.y);
    }
}
