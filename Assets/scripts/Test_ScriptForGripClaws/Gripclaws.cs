using System.Collections;
using UnityEngine;

public class Gripclaws : MonoBehaviour
{
    [Header("Links")]
    public Transform hand;
    public Transform startPoint;
    public Transform playerTransform;
    public Camera playerCamera;
    public LineRenderer rope; // Ссылка на LineRenderer

    [Header("Settings")]
    public float speed = 30f;
    public float returnSpeed = 60f;
    public float maxDistance = 40f;
    public float collisionRadius = 0.5f;

    private Transform originalParent;
    private Quaternion originalRotation;
    private bool isFlying = false;
    private bool isAttached = false;

    void Start()
    {
        originalParent = hand.parent;
        originalRotation = hand.localRotation;

        if (playerTransform == null) playerTransform = transform;
        if (playerCamera == null) playerCamera = Camera.main;

        // Если забыли назначить в инспекторе, попробуем найти на этом же объекте
        if (rope == null) rope = GetComponent<LineRenderer>();

        if (rope != null) rope.enabled = false; // Скрываем трос в начале

        if (hand.GetComponent<Rigidbody>())
            hand.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(InputListener());
    }

    // Добавляем LateUpdate для плавного обновления троса за игроком
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
        rope.SetPosition(0, startPoint.position); // Точка у игрока
        rope.SetPosition(1, hand.position);       // Точка у руки
    }

    IEnumerator InputListener()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !isFlying && !isAttached)
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
                if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                {
                    hand.position = hit.point;
                    hand.forward = hit.normal * -1;
                    yield return StartCoroutine(HandleAttachment(hit));
                    goto ReturnLabel;
                }
                else if (hit.collider.transform != playerTransform)
                {
                    break;
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

        yield return new WaitForSeconds(0.15f); // Защита от двойного клика

        while (true)
        {
            // Выходим из цикла, если нажали ЛКМ или если рука сама отвалилась
            if (Input.GetMouseButtonDown(0)) break;

            float currentDist = Vector3.Distance(playerTransform.position, hand.position);
            if (currentDist > maxDistance + 5f) break; // Запас на разрыв троса

            yield return null;
        }

        isAttached = false;
        hand.SetParent(null);
    }


    void ResetHand()
    {
        if (rope != null) rope.enabled = false; // Прячем трос
        hand.SetParent(originalParent);
        hand.position = startPoint.position;
        hand.rotation = startPoint.rotation;
        isFlying = false;
        isAttached = false;
    }
}