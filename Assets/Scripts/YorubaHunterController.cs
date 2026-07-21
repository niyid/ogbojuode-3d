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

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, meleeRange))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
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
            Destroy(bullet, 3f);
        }
    }

    public void CastEgbe()
    {
        if (Time.time < nextEgbeTime) return;
        nextEgbeTime = Time.time + egbeCooldown;

        if (egbeSmokeFX != null) Instantiate(egbeSmokeFX, transform.position, Quaternion.identity);

        Vector3 destination = transform.position + transform.forward * egbeDistance;
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
