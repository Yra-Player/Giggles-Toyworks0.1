using System.Collections;
using UnityEngine;

public class GripclawsManager : MonoBehaviour
{
    [System.Serializable]
    public class HandData
    {
        public string name = "Hand";
        public Transform handTransform;
        public Transform startPoint;
        public LineRenderer rope;
        public int inputButton = 0;

        [HideInInspector] public Transform originalParent;
        [HideInInspector] public bool isFlying = false;
        [HideInInspector] public bool isAttached = false;
        [HideInInspector] public Rigidbody attachedRB;
        [HideInInspector] public IGripInteractable activeInteractable;
    }

    [Header("Hands Setup")]
    public HandData[] hands;

    [Header("Global Settings")]
    public Transform playerTransform;
    public Camera playerCamera;
    public float speed = 30f;
    public float returnSpeed = 60f;
    public float maxDistance = 40f;
    public float collisionRadius = 0.5f;

    [Header("Physics Setting")]
    public float pullStrength = 150f;
    public string draggableTag = "Box";
    public float shootCooldown = 0.5f;

    [Header("Juiciness & Feedback")]
    [Tooltip("Время (в секундах), на которое рука замирает при попадании в цель, давая игроку шанс зажать кнопку")]
    public float hitStickDuration = 0.35f;

    private float lastDetachTime;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerTransform == null) playerTransform = transform;

        foreach (var hand in hands)
        {
            if (hand.handTransform != null)
            {
                hand.originalParent = hand.handTransform.parent;
                if (hand.rope != null) hand.rope.enabled = false;

                Rigidbody rb = hand.handTransform.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                StartCoroutine(InputListener(hand));
            }
        }
    }

    void LateUpdate()
    {
        foreach (var hand in hands)
        {
            if (hand.handTransform != null && (hand.isFlying || hand.isAttached))
            {
                DrawRope(hand);
            }
        }
    }

    void DrawRope(HandData hand)
    {
        if (hand.rope == null) return;
        hand.rope.enabled = true;
        hand.rope.SetPosition(0, hand.startPoint.position);
        hand.rope.SetPosition(1, hand.handTransform.position);
    }

    IEnumerator InputListener(HandData hand)
    {
        while (true)
        {
            // Проверяем, карабкается ли игрок прямо сейчас
            bool playerIsClimbing = false;
            if (playerTransform != null)
            {
                var movement = playerTransform.GetComponent<FirstPersonMovement>();
                if (movement != null)
                {
                    playerIsClimbing = movement.IsClimbing; // Считываем состояние из скрипта движения
                }
            }

            // Условие выстрела: добавлена проверка !playerIsClimbing (не карабкается)
            if (hand.handTransform.gameObject.activeInHierarchy &&
                Input.GetMouseButtonDown(hand.inputButton) &&
                !hand.isFlying && !hand.isAttached &&
                !playerIsClimbing &&
                Time.time > lastDetachTime + shootCooldown)
            {
                StartCoroutine(ClawRoutine(hand));
            }
            yield return null;
        }
    }

    IEnumerator ClawRoutine(HandData hand)
    {
        hand.isFlying = true;
        Vector3 shootDirection = playerCamera.transform.forward;
        hand.handTransform.SetParent(null);

        while (Vector3.Distance(hand.startPoint.position, hand.handTransform.position) < maxDistance)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy) break;

            Vector3 nextPosition = hand.handTransform.position + shootDirection * speed * Time.deltaTime;
            RaycastHit hit;

            if (Physics.SphereCast(hand.handTransform.position, collisionRadius, shootDirection, out hit, speed * Time.deltaTime + 0.5f))
            {
                if (hit.collider.transform != playerTransform && !hit.collider.transform.IsChildOf(playerTransform))
                {
                    // ДОБАВЛЕНО: поддержка вашего нового тега "LadderHandle"
                    if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag(draggableTag) ||
                        hit.collider.CompareTag("LeverL") || hit.collider.CompareTag("LadderHandle"))
                    {
                        hand.handTransform.position = hit.point;
                        hand.handTransform.forward = hit.normal * -1;
                        yield return StartCoroutine(HandleAttachment(hand, hit));
                        goto ReturnLabel;
                    }
                    else break;
                }
            }
            hand.handTransform.position = nextPosition;
            hand.handTransform.forward = shootDirection;
            yield return null;
        }

    ReturnLabel:
        hand.isFlying = true;
        hand.isAttached = false;
        hand.handTransform.SetParent(null);

        while (Vector3.Distance(hand.handTransform.position, hand.startPoint.position) > 0.3f)
        {
            hand.handTransform.position = Vector3.MoveTowards(hand.handTransform.position, hand.startPoint.position, returnSpeed * Time.deltaTime);
            hand.handTransform.rotation = Quaternion.Slerp(hand.handTransform.rotation, hand.startPoint.rotation, returnSpeed * 0.2f * Time.deltaTime);
            yield return null;
        }
        ResetHand(hand);
    }

    IEnumerator HandleAttachment(HandData hand, RaycastHit hit)
    {
        hand.isAttached = true;
        hand.isFlying = false;
        hand.handTransform.SetParent(hit.transform);
        hand.attachedRB = hit.collider.GetComponent<Rigidbody>();

        LeftLever leftLever = hit.collider.GetComponent<LeftLever>();
        hand.activeInteractable = hit.collider.GetComponent<IGripInteractable>();

        if (hand.activeInteractable != null)
        {
            hand.activeInteractable.OnGripStart(hand.inputButton);
        }

        float stickTimer = 0f;
        while (stickTimer < hitStickDuration)
        {
            stickTimer += Time.deltaTime;
            if (Input.GetMouseButton(hand.inputButton))
                break;

            yield return null;
        }

        float pressTimer = 0;

        while (hand.isAttached)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy || hit.collider == null) break;

            float currentDist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
            if (currentDist > maxDistance + 5f || currentDist < 3.2f) break;

            if (Input.GetMouseButton(hand.inputButton))
            {
                pressTimer += Time.deltaTime;

                if (leftLever != null)
                {
                    leftLever.PullLever();
                }
                else if (pressTimer > 0.15f && hand.attachedRB != null)
                {
                    Vector3 dir = hand.startPoint.position - hand.handTransform.position;
                    hand.attachedRB.AddForce(dir.normalized * pullStrength, ForceMode.Acceleration);
                }
            }
            else
            {
                if (leftLever != null)
                {
                    leftLever.ResetLever();
                }

                if (hand.activeInteractable != null)
                {
                    hand.activeInteractable.OnGripStop(); // Корректно вызываем остановку взаимодействия
                }

                if (pressTimer > 0.01f && pressTimer <= 0.15f) break;
                break;
            }
            yield return null;
        }

        if (leftLever != null)
        {
            leftLever.ResetLever();
        }

        if (hand.activeInteractable != null)
        {
            hand.activeInteractable.OnGripStop();
        }

        lastDetachTime = Time.time;
        hand.isAttached = false;
        hand.handTransform.SetParent(null);
    }

    void ResetHand(HandData hand)
    {
        if (hand.rope != null) hand.rope.enabled = false;
        hand.handTransform.SetParent(hand.originalParent);
        hand.handTransform.position = hand.startPoint.position;
        hand.handTransform.rotation = hand.startPoint.rotation;
        hand.isFlying = false;
        hand.isAttached = false;
        hand.attachedRB = null;
    }
}
