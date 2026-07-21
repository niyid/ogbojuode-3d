using UnityEngine;

// One AI drives all three forest creatures — Agbako, Ijamba, and Eru differ
// in stats/scale/color, not in behavior logic. Keeps three "monsters" from
// becoming three copy-pasted scripts.
public class CreatureAI : MonoBehaviour, IDamageable
{
    public enum CreatureType { Agbako, Ijamba, Eru }

    [Header("Identity")]
    public CreatureType creatureType = CreatureType.Agbako;

    [Header("Stats")]
    public float chaseSpeed = 4.0f;
    public float aggroRange = 25.0f;
    public float attackRange = 3.0f;
    public int maxHealth = 100;
    public int contactDamage = 15;

    private int currentHealth;
    private Transform player;
    private float nextContactHitTime;
    private const float contactHitCooldown = 1.0f;

    void Start()
    {
        currentHealth = maxHealth;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > aggroRange) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);

        if (dist > attackRange)
        {
            transform.position += transform.forward * chaseSpeed * Time.deltaTime;
        }
        else if (Time.time >= nextContactHitTime)
        {
            nextContactHitTime = Time.time + contactHitCooldown;
            IDamageable target = player.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(contactDamage);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    // Presets matching the source material: Agbako towers over the other two,
    // Ijamba is a slower brute-force hitter, Eru is fast and erratic.
    public static void ApplyPreset(CreatureAI ai, CreatureType type)
    {
        ai.creatureType = type;
        switch (type)
        {
            case CreatureType.Agbako:
                ai.maxHealth = 150; ai.chaseSpeed = 3.5f; ai.contactDamage = 20;
                break;
            case CreatureType.Ijamba:
                ai.maxHealth = 200; ai.chaseSpeed = 2.5f; ai.contactDamage = 30;
                break;
            case CreatureType.Eru:
                ai.maxHealth = 80; ai.chaseSpeed = 6.0f; ai.contactDamage = 10;
                break;
        }
    }
}
