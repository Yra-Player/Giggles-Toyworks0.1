using UnityEngine;

public class Crouch : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftControl;

    [Header("Slow Movement")]
    public FirstPersonMovement movement;
    public float movementSpeed = 2f;

    [Header("Low Head")]
    public Transform headToLower;
    public float crouchYHeadPosition = 1f;
    public float crouchSpeed = 10f;

    [Header("Collider")]
    public CapsuleCollider colliderToLower;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;

    private float defaultHeadY;
    private float defaultColliderHeight;

    void Start()
    {
        // Кэшируем начальные размеры один раз при старте
        if (headToLower) defaultHeadY = headToLower.localPosition.y;
        if (colliderToLower) defaultColliderHeight = colliderToLower.height;
    }

    void Update()
    {
        // Считываем кнопку в Update и сразу меняем состояние
        bool crouchInput = Input.GetKey(key);

        if (crouchInput && !IsCrouched)
        {
            IsCrouched = true;
            SetSpeedOverrideActive(true);
            CrouchStart?.Invoke();
        }
        else if (!crouchInput && IsCrouched)
        {
            IsCrouched = false;
            SetSpeedOverrideActive(false);
            CrouchEnd?.Invoke();
        }

        // Плавное изменение высоты теперь работает в Update для идеальной отзывчивости камеры
        HandleCrouchVisuals();
    }

    void HandleCrouchVisuals()
    {
        float targetHeadY = IsCrouched ? crouchYHeadPosition : defaultHeadY;

        // Разница, на которую нужно уменьшить коллайдер
        float loweringAmount = defaultHeadY - crouchYHeadPosition;
        float targetHeight = IsCrouched ? Mathf.Max(defaultColliderHeight - loweringAmount, 0.1f) : defaultColliderHeight;

        // Плавно двигаем голову/камеру
        if (headToLower)
        {
            float newHeadY = Mathf.MoveTowards(headToLower.localPosition.y, targetHeadY, crouchSpeed * Time.deltaTime);
            headToLower.localPosition = new Vector3(headToLower.localPosition.x, newHeadY, headToLower.localPosition.z);
        }

        // Плавно меняем высоту физического коллайдера
        if (colliderToLower)
        {
            colliderToLower.height = Mathf.MoveTowards(colliderToLower.height, targetHeight, crouchSpeed * Time.deltaTime);
            colliderToLower.center = Vector3.up * (colliderToLower.height * 0.5f);
        }
    }

    void SetSpeedOverrideActive(bool state)
    {
        if (!movement) return;

        if (state)
        {
            if (!movement.speedOverrides.Contains(SpeedOverride))
                movement.speedOverrides.Add(SpeedOverride);
        }
        else
        {
            movement.speedOverrides.Remove(SpeedOverride);
        }
    }

    float SpeedOverride() => movementSpeed;
}
