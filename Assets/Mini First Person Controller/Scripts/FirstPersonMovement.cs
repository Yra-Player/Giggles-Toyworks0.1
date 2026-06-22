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

    public bool IsRunning { get; private set; }

    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool runInput;

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
