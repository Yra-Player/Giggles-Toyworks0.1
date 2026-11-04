using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Jump : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Jump Settings")]
    public float jumpForce = 7.5f; // НАСТРОЙКА ВЫСОТЫ ПРЫЖКА С ЗЕМЛИ
    [Tooltip("Множитель гравитации при падении. Убирает эффект космоса.")]
    public float fallMultiplier = 2.5f;

    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    private GroundCheck groundCheck;

    private bool jumpRequested;

    void Reset()
    {
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Ловим нажатие пробела
        if (Input.GetButtonDown("Jump") && (!groundCheck || groundCheck.isGrounded))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (jumpRequested)
        {
            // Обнуляем скорость по Y перед импульсом для стабильной высоты
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            Jumped?.Invoke();
            jumpRequested = false;
        }

        // Реалистичное тяжелое падение вниз после прохождения пика прыжка
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
