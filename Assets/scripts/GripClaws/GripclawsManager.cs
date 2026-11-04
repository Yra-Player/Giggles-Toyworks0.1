using UnityEngine;

[RequireComponent(typeof(GripclawPhysics))]
[RequireComponent(typeof(GripclawVisuals))]
public class GripclawsManager : MonoBehaviour
{
    [Header("Hands Setup")]
    public GripclawHand[] hands;

    [Header("Global Settings")]
    public Transform playerTransform;
    public Camera playerCamera;
    public float speed = 75f;
    public float returnSpeed = 200f;
    public float maxDistance = 55f;
    public float collisionRadius = 0.5f;

    [Header("Physics Setting")]
    public float pullStrength = 25f;
    public string draggableTag = "Box";
    public float shootCooldown = 0.5f;

    [Header("Juiciness & Feedback")]
    public float hitStickDuration = 0.35f;

    [Header("UI & Aiming")]
    public UnityEngine.UI.Image distanceIndicator;

    private GripclawPhysics physicsModule;
    private GripclawVisuals visualsModule;
    private float lastDetachTime;

    void Awake()
    {
        physicsModule = GetComponent<GripclawPhysics>();
        visualsModule = GetComponent<GripclawVisuals>();
    }

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerTransform == null) playerTransform = transform;
    }

    void Update()
    {
        if (Time.time < lastDetachTime + shootCooldown) return;

        bool playerIsClimbing = false;
        bool playerIsPulling = false;

        if (playerTransform != null)
        {
            var movement = playerTransform.GetComponent<FirstPersonMovement>();
            if (movement != null)
            {
                playerIsClimbing = movement.IsClimbing;
                playerIsPulling = movement.IsPullingUp;
            }
        }

        if (playerIsClimbing || playerIsPulling) return;

        foreach (var hand in hands)
        {
            if (hand != null && hand.handTransform.gameObject.activeInHierarchy && !hand.isFlying && !hand.isAttached)
            {
                if (Input.GetMouseButtonDown(hand.inputButton))
                {
                    physicsModule.LaunchHand(hand);
                }
            }
        }
    }

    void LateUpdate()
    {
        visualsModule.RenderHandVisuals(hands);
        UpdateUIIndicator();
    }

    private void UpdateUIIndicator()
    {
        if (distanceIndicator == null) return;

        float maxCurrentDist = 0f;
        bool anyHandActive = false;

        foreach (var hand in hands)
        {
            // Проверяем существование руки и её компонентов
            if (hand != null && hand.handTransform != null)
            {
                // Берем флаги активности напрямую из runtime-состояния руки
                if (hand.isFlying || hand.isAttached)
                {
                    anyHandActive = true;

                    // Считаем расстояние от точки вылета до текущего положения летящей ладони
                    float dist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
                    if (dist > maxCurrentDist) maxCurrentDist = dist;
                }
            }
        }

        if (anyHandActive)
        {
            // Вычисляем процент заполнения круга (от 1 до 0 по мере удаления руки)
            float fillRatio = 1f - (maxCurrentDist / maxDistance);
            distanceIndicator.fillAmount = Mathf.Clamp01(fillRatio);
        }
        else
        {
            // Если руки возвращены или не активны, индикатор полностью заполнен (или скрыт)
            distanceIndicator.fillAmount = 1f;
        }
    }


    public void SetDetachCooldown()
    {
        lastDetachTime = Time.time;
    }

}
