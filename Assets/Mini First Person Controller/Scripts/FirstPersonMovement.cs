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
    public float climbSpeed = 4f; // Скорость подъема по лестнице

    public bool IsRunning { get; private set; }

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool runInput;
    public bool IsClimbing = false; // Находится ли игрок на лестнице

    // Ссылка на компонент проверки земли, чтобы знать, когда мы летим
    private GroundCheck playerGroundCheck;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Защита от случайного падения персонажа на бок

        // Находим GroundCheck в дочерних объектах игрока (так же, как это делает ваш скрипт Jump)
        playerGroundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Update()
    {
        // Опрашиваем кнопки строго в Update (работает без пропусков)
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        runInput = Input.GetKey(runningKey);
    }

    void FixedUpdate()
    {
        if (IsClimbing)
        {
            // --- ЛОГИКА КАРАБКАНИЯ ПО ЛЕСТНИЦЕ ---
            rb.useGravity = false;

            Vector3 climbVelocity = new Vector3(0, inputVector.y * climbSpeed, 0);
            Vector3 horizontalMove = transform.rotation * new Vector3(inputVector.x, 0, 0).normalized * speed;

            rb.linearVelocity = climbVelocity + horizontalMove;
        }
        else
        {
            // --- ОБЫЧНОЕ ДВИЖЕНИЕ И ПАДЕНИЕ ---
            rb.useGravity = true;

            // Если датчик земли существует и сообщает, что мы НЕ на земле (летим/падаем)
            if (playerGroundCheck != null && !playerGroundCheck.isGrounded)
            {
                // Полностью блокируем WASD-управление. Rigidbody летит чисто по физической инерции падения.
                return;
            }

            IsRunning = canRun && runInput;

            float targetMovingSpeed = IsRunning ? runSpeed : speed;
            if (speedOverrides.Count > 0)
            {
                targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
            }

            // Нормализуем вектор, чтобы скорость по диагонали не удваивалась
            Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y).normalized;
            Vector3 targetVelocity = transform.rotation * moveDirection * targetMovingSpeed;

            // Удерживаем текущую скорость прыжка/падения
            targetVelocity.y = rb.linearVelocity.y;

            rb.linearVelocity = targetVelocity;
        }
    }

    // --- ОТСЛЕЖИВАНИЕ ЗОНЫ ЛЕСТНИЦЫ ---
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
