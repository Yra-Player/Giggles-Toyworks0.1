using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Jump : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Jump Settings")]
    public float jumpForce = 7.5f;
    [Tooltip("Множитель гравитации при падении. Убирает эффект космоса.")]
    public float fallMultiplier = 2.5f;
    [Tooltip("Множитель гравитации при коротком нажатии пробела.")]
    public float lowJumpMultiplier = 2f;

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
        // Ловим нажатие пробела в Update, чтобы прыжок не залипал
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

        // Продвинутая гравитация, которая убирает ощущение космоса
        if (rb.linearVelocity.y < 0)
        {
            // Персонаж падает быстрее и ощущается тяжелым
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Если игрок отпустил пробел раньше времени — прыжок срезается (механика как в Марио)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
