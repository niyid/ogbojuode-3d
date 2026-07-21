using UnityEngine;

// FireMusket() previously just spawned a Rigidbody with velocity and let it
// fly for 3 seconds — nothing ever detected a hit, so the musket dealt zero
// damage no matter what it struck. YorubaHunterController.FireMusket()
// attaches this automatically, so any bulletPrefab works (even the plain
// "sphere with a Rigidbody" suggested in the README) without needing to be
// pre-configured in the editor.
[RequireComponent(typeof(Rigidbody))]
public class MusketBullet : MonoBehaviour
{
    public int damage = 20;

    private void Awake()
    {
        // Bullets are fast and thin; discrete collision can tunnel through
        // thin colliders between physics steps. Continuous detection avoids
        // "the bullet visibly passed through the creature but nothing
        // happened" bugs.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Collider col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Never hurt whoever fired it.
        if (other.CompareTag("Player")) return;

        // IDamageable sits on the enemy's root GameObject, while the collider
        // struck is usually on a child model/placeholder — walk up.
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
