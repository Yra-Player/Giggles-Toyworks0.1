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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Защита от случайного падения персонажа на бок
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
            // Полностью гасим обычную физику и гравитацию, чтобы игрок не соскальзывал вниз
            rb.useGravity = false;

            // Двигаем игрока вверх-вниз в зависимости от нажатия W / S (inputVector.y)
            Vector3 climbVelocity = new Vector3(0, inputVector.y * climbSpeed, 0);

            // Также позволяем игроку немного подруливать по горизонтали (вбок), чтобы сойти с лестницы на площадку
            Vector3 horizontalMove = transform.rotation * new Vector3(inputVector.x, 0, 0).normalized * speed;

            rb.linearVelocity = climbVelocity + horizontalMove;
        }
        else
        {
            // --- ОБЫЧНОЕ ДВИЖЕНИЕ НА ЗЕМЛЕ ---
            rb.useGravity = true;

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
        // Проверяем тег триггера, который вы создали внутри префаба лестницы
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
