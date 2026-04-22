using System.Collections;
using UnityEngine;

public class Gripclaws : MonoBehaviour
{
    [Header("Links")]
    public Transform hand;
    public Transform startPoint;
    public Transform playerTransform;
    public Camera playerCamera;
    public LineRenderer rope;

    [Header("Settings")]
    public int inputButton = 0; // 0 для ЛКМ, 1 для ПКМ
    public float speed = 30f;
    public float returnSpeed = 60f;
    public float maxDistance = 40f;
    public float collisionRadius = 0.5f;

    private Transform originalParent;
    private bool isFlying = false;
    private bool isAttached = false;

    void Start()
    {
        originalParent = hand.parent;

        if (playerTransform == null) playerTransform = transform;
        if (playerCamera == null) playerCamera = Camera.main;
        if (rope == null) rope = GetComponent<LineRenderer>();

        if (rope != null) rope.enabled = false;

        if (hand.GetComponent<Rigidbody>())
            hand.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(InputListener());
    }

    void LateUpdate()
    {
        if (isFlying || isAttached)
        {
            DrawRope();
        }
    }

    void DrawRope()
    {
        if (rope == null) return;
        rope.enabled = true;
        rope.SetPosition(0, startPoint.position);
        rope.SetPosition(1, hand.position);
    }

    IEnumerator InputListener()
    {
        while (true)
        {
            // Используем переменную inputButton вместо 0
            if (Input.GetMouseButtonDown(inputButton) && !isFlying && !isAttached)
            {
                yield return StartCoroutine(ClawRoutine());
            }
            yield return null;
        }
    }

    IEnumerator ClawRoutine()
    {
        isFlying = true;
        Vector3 shootDirection = playerCamera.transform.forward;
        hand.SetParent(null);

        // --- ВПЕРЕД ---
        while (Vector3.Distance(startPoint.position, hand.position) < maxDistance)
        {
            Vector3 nextPosition = hand.position + shootDirection * speed * Time.deltaTime;

            RaycastHit hit;
            if (Physics.SphereCast(hand.position, collisionRadius, shootDirection, out hit, speed * Time.deltaTime + 0.5f))
            {
                // Проверяем, чтобы не попасть в самого себя или другую руку
                if (hit.collider.transform != playerTransform && !hit.collider.transform.IsChildOf(playerTransform))
                {
                    if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                    {
                        hand.position = hit.point;
                        hand.forward = hit.normal * -1;
                        yield return StartCoroutine(HandleAttachment(hit));
                        goto ReturnLabel;
                    }
                    else
                    {
                        break; // Попали в обычную стену
                    }
                }
            }

            hand.position = nextPosition;
            hand.forward = shootDirection;
            yield return null;
        }

    ReturnLabel:
        isFlying = true;

        // --- ВОЗВРАТ ---
        while (Vector3.Distance(hand.position, startPoint.position) > 0.3f)
        {
            hand.position = Vector3.MoveTowards(hand.position, startPoint.position, returnSpeed * Time.deltaTime);
            hand.rotation = Quaternion.Slerp(hand.rotation, startPoint.rotation, returnSpeed * 0.2f * Time.deltaTime);
            yield return null;
        }

        ResetHand();
    }

    IEnumerator HandleAttachment(RaycastHit hit)
    {
        isAttached = true;
        isFlying = false;
        hand.SetParent(hit.transform);

        yield return new WaitForSeconds(0.15f);

        while (true)
        {
            // Отпускаем при повторном нажатии ТУ ЖЕ кнопку
            if (Input.GetMouseButtonDown(inputButton)) break;

            float currentDist = Vector3.Distance(startPoint.position, hand.position);
            if (currentDist > maxDistance + 5f) break;

            yield return null;
        }

        isAttached = false;
        hand.SetParent(null);
    }

    void ResetHand()
    {
        if (rope != null) rope.enabled = false;
        hand.SetParent(originalParent);
        hand.position = startPoint.position;
        hand.rotation = startPoint.rotation;
        isFlying = false;
        isAttached = false;
    }
}
