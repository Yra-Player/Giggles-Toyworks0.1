using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Base Speed")]
    public float speed = 5f;
    public float runSpeed = 9f;
    public bool canRun = true;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Ladder Settings")]
    public float climbSpeed = 4f;

    public bool IsRunning { get; private set; }

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool runInput;
    public bool IsClimbing = false;

    private GroundCheck playerGroundCheck;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerGroundCheck = GetComponentInChildren<GroundCheck>();

        // Обновлено под новую версию Unity: используем PhysicsMaterial вместо PhysicMaterial
        CapsuleCollider playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider != null)
        {
            PhysicsMaterial noFrictionMat = new PhysicsMaterial("NoFriction");
            noFrictionMat.staticFriction = 0f;
            noFrictionMat.dynamicFriction = 0f;
            noFrictionMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            playerCollider.material = noFrictionMat;
        }
    }

    void Update()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        runInput = Input.GetKey(runningKey);
    }

    void FixedUpdate()
    {
        if (IsClimbing)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Возвращаем обычные ограничения
            Vector3 climbVelocity = new Vector3(0, inputVector.y * climbSpeed, 0);
            Vector3 horizontalMove = transform.rotation * new Vector3(inputVector.x, 0, 0).normalized * speed;
            rb.linearVelocity = climbVelocity + horizontalMove;
        }
        else
        {
            rb.useGravity = true;

            // Если летим/падаем — управление отключается
            if (playerGroundCheck != null && !playerGroundCheck.isGrounded)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                return;
            }

            // ЖЕСТКАЯ БЛОКИРОВКА НА РАМПЕ / ЛЕСТНИЦЕ
            // Если игрок на земле и НЕ нажимает WASD
            if (inputVector.sqrMagnitude < 0.01f)
            {
                rb.linearVelocity = Vector3.zero;
                // Замораживаем позицию по X, Y, Z, чтобы физика и гравитация не могли сдвинуть игрока
                rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                return;
            }

            // Если игрок идет, возвращаем стандартное состояние (заморожено только вращение)
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            IsRunning = canRun && runInput;

            float targetMovingSpeed = IsRunning ? runSpeed : speed;
            if (speedOverrides.Count > 0)
            {
                targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
            }

            Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y).normalized;
            Vector3 targetVelocity = transform.rotation * moveDirection * targetMovingSpeed;

            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            IsClimbing = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            IsClimbing = false;
        }
    }
}
