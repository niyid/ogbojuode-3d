using UnityEngine;

// The Ostrich-King (Oba Eye): human from the neck up, giant bird below.
// Unlike Agbako/Ijamba/Eru, he doesn't chase — he holds ground and poses a
// riddle. Refuse or answer wrong enough times and he attacks; answer right
// and he yields passage/wisdom. This is the "moral riddle" half of the
// book's core loop, not just another combat encounter.
[RequireComponent(typeof(RiddleGiver))]
public class OstrichKingBoss : MonoBehaviour, IDamageable
{
    public int maxHealth = 250;
    public int contactDamage = 25;
    public float aggroRange = 6f; // short — he doesn't hunt, you have to approach him

    private int currentHealth;
    private Transform player;
    private bool riddleFailed = false;

    void Start()
    {
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || !riddleFailed) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= 2.5f)
        {
            IDamageable target = player.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(contactDamage);
        }
    }

    // Called by RiddleGiver when the player fails the riddle outright
    // (rather than just walking away, which does nothing).
    public void OnRiddleFailed()
    {
        riddleFailed = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }
}
