using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class YorubaHunterController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6.0f;
    public float rotationSpeed = 720.0f;
    public float gravity = -9.81f;

    [Header("Musket")]
    public Transform musketMuzzle;
    public GameObject bulletPrefab;
    public float musketFireRate = 1.2f;
    public float bulletSpeed = 25f;
    public int musketDamage = 20;
    private float nextFireTime = 0f;

    [Header("Egbe Spell (Teleport)")]
    public float egbeDistance = 8.0f;
    public float egbeCooldown = 3.0f;
    public ParticleSystem egbeSmokeFX;
    private float nextEgbeTime = 0f;

    [Header("Machete")]
    public float meleeRange = 2.0f;
    public int meleeDamage = 25;
    private bool isAttacking = false;

    // Mobile input support - set by touch UI, left at zero for keyboard/editor testing
    public Vector2 MobileMoveInput = Vector2.zero;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleCombatInput();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal") + MobileMoveInput.x;
        float v = Input.GetAxis("Vertical") + MobileMoveInput.y;
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (controller.isGrounded)
        {
            velocity = inputDir * moveSpeed;
            if (inputDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(inputDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCombatInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(SwingMachete());

        if (Input.GetMouseButtonDown(1) && Time.time >= nextFireTime)
            FireMusket();

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextEgbeTime)
            CastEgbe();
    }

    private IEnumerator SwingMachete()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.25f);

        // SphereCast instead of a hairline Raycast: the enemy's collider is
        // usually on a child model/placeholder object, not on the same
        // GameObject as the machete swing origin, so a 1-pixel-wide ray
        // easily "misses" targets that are visually right in front of you.
        // A small radius gives forgiving, expected melee collision.
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, transform.forward, out hit, meleeRange))
        {
            // IDamageable (CreatureAI, OstrichKingBoss) lives on the enemy's
            // root GameObject, while the collider we just hit is usually on
            // a child (the instantiated model or placeholder primitive).
            // GetComponent alone would always return null here — walk up
            // the hierarchy instead.
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(meleeDamage);
        }

        yield return new WaitForSeconds(0.15f);
        isAttacking = false;
    }

    public void FireMusket()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + musketFireRate;

        if (musketMuzzle != null && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, musketMuzzle.position, musketMuzzle.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = musketMuzzle.forward * bulletSpeed;

            // Give the projectile actual collision/damage behavior even if
            // bulletPrefab is just a bare sphere+Rigidbody, per the README's
            // "any small sphere with a Rigidbody works" setup instructions.
            MusketBullet musketBullet = bullet.GetComponent<MusketBullet>();
            if (musketBullet == null) musketBullet = bullet.AddComponent<MusketBullet>();
            musketBullet.damage = musketDamage;

            Destroy(bullet, 3f);
        }
    }

    public void CastEgbe()
    {
        if (Time.time < nextEgbeTime) return;
        nextEgbeTime = Time.time + egbeCooldown;

        if (egbeSmokeFX != null) Instantiate(egbeSmokeFX, transform.position, Quaternion.identity);

        // Blinking straight through solid geometry (a fence, a hut wall, a
        // tree trunk) is a collision-detection gap, not a feature — check
        // the path first and stop just short of anything solid instead.
        Vector3 origin = transform.position + Vector3.up;
        Vector3 destination = transform.position + transform.forward * egbeDistance;
        RaycastHit obstacleHit;
        if (Physics.Raycast(origin, transform.forward, out obstacleHit, egbeDistance))
        {
            float safeDistance = Mathf.Max(0f, obstacleHit.distance - 0.5f);
            destination = transform.position + transform.forward * safeDistance;
        }

        controller.enabled = false;
        transform.position = destination;
        controller.enabled = true;

        if (egbeSmokeFX != null) Instantiate(egbeSmokeFX, transform.position, Quaternion.identity);
    }

    // Called by on-screen mobile buttons
    public void TriggerMacheteAttackButton()
    {
        if (!isAttacking) StartCoroutine(SwingMachete());
    }
}
