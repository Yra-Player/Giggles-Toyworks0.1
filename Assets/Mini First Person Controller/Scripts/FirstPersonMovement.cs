using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonMovement : MonoBehaviour
{
    [HideInInspector]
    public Vector3 activeConveyorVelocity = Vector3.zero;
    public bool IsPullingUp = false;
    private Vector3 pullTargetPosition; // Конечная точка над перекладиной
    private float pullSpeed;

    [Header("Base Speed")]
    public float speed = 5f;
    public float runSpeed = 9f;
    public bool canRun = true;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Ladder Settings")]
    public float climbSpeed = 4f;
    public float jumpForce = 5f; // Отвечает ИСКЛЮЧИТЕЛЬНО за прыжок / отталкивание от стремянки!
    public KeyCode jumpKey = KeyCode.Space;

    private float climbCooldownTimer = 0f;
    private const float CLIMB_COOLDOWN_TIME = 0.5f;

    private bool jumpInput;

    public bool IsRunning { get; private set; }
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool runInput;

    public bool IsClimbing = false;
    private Transform activeLadderTransform;

    [HideInInspector]
    public bool isOnConveyor = false;
    private GroundCheck playerGroundCheck;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerGroundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Update()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        runInput = Input.GetKey(runningKey);

        if (Input.GetKeyDown(jumpKey))
        {
            jumpInput = true;
        }

        if (Time.timeScale == 0f)
        {
            inputVector = Vector2.zero;
            runInput = false;
            jumpInput = false;
            return;
        }

        if (climbCooldownTimer > 0f)
        {
            climbCooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        //Система подтягивания
        if (IsPullingUp)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            //Перемещение тела к цели
            Vector3 newPosition = Vector3.MoveTowards(rb.position, pullTargetPosition, pullSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Прыжок во время карабканья по стремянке
        if (IsClimbing && jumpInput)
        {
            PerformLadderJump();
            jumpInput = false;
            return;
        }

        if (IsClimbing && activeLadderTransform != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            Vector3 climbDirection = activeLadderTransform.up * inputVector.y;
            Vector3 strafeDirection = activeLadderTransform.right * inputVector.x;

            rb.linearVelocity = (climbDirection + strafeDirection).normalized * climbSpeed;
        }
        else
        {
            rb.useGravity = true;

            if (playerGroundCheck != null && !playerGroundCheck.isGrounded)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                jumpInput = false;
                return;
            }

            // ИСПРАВЛЕННАЯ БЛОКИРОВКА: Замораживаем только X и Z, ось Y не трогаем!
            if (inputVector.sqrMagnitude < 0.01f && !isOnConveyor)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                jumpInput = false;
                return;
            }

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            IsRunning = canRun && runInput;

            float targetMovingSpeed = IsRunning ? runSpeed : speed;
            if (speedOverrides.Count > 0)
            {
                targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
            }

            Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y).normalized;
            Vector3 targetVelocity = transform.rotation * moveDirection * targetMovingSpeed;

            if (isOnConveyor)
            {
                targetVelocity += activeConveyorVelocity;
            }

            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }

        jumpInput = false;
    }

    private void PerformLadderJump()
    {
        IsClimbing = false;
        activeLadderTransform = null;
        climbCooldownTimer = CLIMB_COOLDOWN_TIME;

        // Импульс строго вверх при прыжке со стремянки
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder") && climbCooldownTimer <= 0f)
        {
            IsClimbing = true;
            activeLadderTransform = other.transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ladder") && !IsClimbing && climbCooldownTimer <= 0f && inputVector.y > 0)
        {
            IsClimbing = true;
            activeLadderTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            if (inputVector.y > 0 && activeLadderTransform != null)
            {
                Vector3 pushDirection = transform.forward;
                pushDirection.y = 0;
                pushDirection.Normalize();

                rb.linearVelocity = Vector3.zero;
                rb.AddForce(pushDirection * 5f + Vector3.up * 2.5f, ForceMode.VelocityChange);
                climbCooldownTimer = CLIMB_COOLDOWN_TIME;
            }

            IsClimbing = false;
            activeLadderTransform = null;
        }
    }

    public void StartPulling(Vector3 targetPos, float speedValue)
    {
        IsPullingUp = true;
        pullTargetPosition = targetPos;
        pullSpeed = speedValue;
    }
    public void StopPulling()
    {
        IsPullingUp = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Рассчитываем вектор отталкивания на основе WASD ввода в момент отрыва
        Vector3 localMoveDir = new Vector3(inputVector.x, 0, inputVector.y).normalized;
        Vector3 worldLaunchDir;

        if (localMoveDir.sqrMagnitude > 0.01f)
        {
            //Если игрок жмет WASD толкает в сторону зажатых клавиш
            worldLaunchDir = transform.rotation * localMoveDir;
        }
        else
        {
            // Если ничего не зажато — аккуратно подталкиваем строго вперед по взгляду
            worldLaunchDir = transform.forward;
            worldLaunchDir.y = 0;
            worldLaunchDir.Normalize();
        }
        // Сила заталкивания в вентиляцию и сильный подброс вверх, чтобы не застрять в текстурах пола люка
        float forwardForce = 9f;
        float upwardForce = 6.5f;

        Vector3 finalVelosity = (worldLaunchDir * forwardForce) + (Vector3.up * upwardForce);

        rb.linearVelocity = finalVelosity;
    }
}
