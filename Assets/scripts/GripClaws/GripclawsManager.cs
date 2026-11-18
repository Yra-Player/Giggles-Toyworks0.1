using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GripclawPhysics))]
[RequireComponent(typeof(GripclawVisuals))]
[RequireComponent(typeof(GripclawAudio))]
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
    private GripclawAudio audioModule;
    private CharacterController playerController;
    private Rigidbody playerRigidbody;
    private float lastDetachTime;

    void Awake()
    {
        physicsModule = GetComponent<GripclawPhysics>();
        visualsModule = GetComponent<GripclawVisuals>();
        audioModule = GetComponent<GripclawAudio>();
    }

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerTransform == null) playerTransform = transform;

        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<CharacterController>();
            playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        }
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
                    if (audioModule != null) audioModule.PlayFlyAudio(hand);
                }
            }
        }
    }

    void LateUpdate()
    {
        EnforceCableTension();
        visualsModule.RenderHandVisuals(hands);

        if (audioModule != null)
        {
            foreach (var hand in hands)
            {
                if (hand != null && hand.isFlying)
                {
                    audioModule.UpdateFlyTensionAudio(hand);
                }
            }
        }

        UpdateUIIndicator();
    }

    // Блокирует дальнейшее удаление игрока, когда кабель натянут до предела, а кнопка зажата
    private void EnforceCableTension()
    {
        if (playerTransform == null) return;

        foreach (var hand in hands)
        {
            if (hand == null || !hand.isAttached || hand.handTransform == null) continue;
            if (!Input.GetMouseButton(hand.inputButton)) continue;

            // Если рука держит легкую коробку, которую можно тянуть — не стопорим игрока
            if (hand.attachedRB != null && !hand.attachedRB.isKinematic) continue;

            float usedLength = hand.GetTotalCableLength();
            if (usedLength > maxDistance)
            {
                float excess = usedLength - maxDistance;

                // Определяем ближайшую к игроку точку крепления (первый угол изгиба или саму руку)
                Vector3 anchorPoint = hand.bendPoints.Count > 0
                    ? hand.bendPoints.First()
                    : hand.handTransform.position;

                Vector3 originPos = hand.ropeStartPoint != null ? hand.ropeStartPoint.position : hand.startPoint.position;
                Vector3 pullBackDir = (anchorPoint - originPos).normalized;

                // Не даем утягивать игрока под пол или в потолок по оси Y, если он просто идет
                pullBackDir.y = 0f;
                if (pullBackDir.sqrMagnitude > 0.001f)
                {
                    pullBackDir.Normalize();
                    Vector3 correction = pullBackDir * excess;

                    if (playerController != null && playerController.enabled)
                    {
                        playerController.Move(correction);
                    }
                    else if (playerRigidbody != null && !playerRigidbody.isKinematic)
                    {
                        playerRigidbody.MovePosition(playerRigidbody.position + correction);
                    }
                    else
                    {
                        playerTransform.position += correction;
                    }
                }
            }
        }
    }

    private void UpdateUIIndicator()
    {
        if (distanceIndicator == null) return;

        float maxUsedCable = 0f;
        bool anyHandActive = false;

        foreach (var hand in hands)
        {
            if (hand != null && hand.handTransform != null)
            {
                if (hand.isFlying || hand.isAttached)
                {
                    anyHandActive = true;

                    float usedLength = hand.GetTotalCableLength();
                    if (usedLength > maxUsedCable) maxUsedCable = usedLength;
                }
            }
        }

        if (anyHandActive)
        {
            float fillRatio = 1f - (maxUsedCable / maxDistance);
            distanceIndicator.fillAmount = Mathf.Clamp01(fillRatio);
        }
        else
        {
            distanceIndicator.fillAmount = 1f;
        }
    }

    public void SetDetachCooldown()
    {
        lastDetachTime = Time.time;
    }
}
