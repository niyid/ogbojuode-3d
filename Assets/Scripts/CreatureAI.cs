using UnityEngine;

// One AI drives all combat creatures in a given world; they differ in
// stats/scale/color via a role preset, not in behavior logic. Keeps three
// "monsters" from becoming three copy-pasted scripts. Presets are named by
// combat role (Swift/Brute/Titan) rather than any specific story's creature
// names, so this same script serves both ogbojuode-3d and irekeonibudo-3d —
// the SceneSetupWizard in each project assigns the in-world display name
// (e.g. "Warrior_Fish_Eja_Jagunjagun") separately from the stat role.
public class CreatureAI : MonoBehaviour, IDamageable
{
    public enum CreatureType { Titan, Brute, Swift }

    [Header("Identity")]
    public CreatureType creatureType = CreatureType.Titan;

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

    // Titan: towering, slow, highest damage (Agbako / the warrior-fish guard).
    // Brute: hulking, slower, heavy hits (Ijamba / the wrestler-cat).
    // Swift: fast, fragile, low damage (Eru / the flying snake).
    public static void ApplyPreset(CreatureAI ai, CreatureType type)
    {
        ai.creatureType = type;
        switch (type)
        {
            case CreatureType.Titan:
                ai.maxHealth = 150; ai.chaseSpeed = 3.5f; ai.contactDamage = 20;
                break;
            case CreatureType.Brute:
                ai.maxHealth = 200; ai.chaseSpeed = 2.5f; ai.contactDamage = 30;
                break;
            case CreatureType.Swift:
                ai.maxHealth = 80; ai.chaseSpeed = 6.0f; ai.contactDamage = 10;
                break;
        }
    }
}
