using System.Collections;
using UnityEngine;

public class Gripclaws : MonoBehaviour
{
    [Header("Links")]
    public Transform hand;          // Летающая кисть
    public Transform gunBarrel;     // Поворотная часть пистолета
    public Transform startPoint;    // Точка на кончике ствола
    public Transform playerTransform;
    public Camera playerCamera;
    public LineRenderer rope;

    [Header("Settings")]
    public float speed = 35f;
    public float returnSpeed = 70f;
    public float maxDistance = 40f;
    public float collisionRadius = 0.4f;

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
        if (rope == null) rope = GetComponent<LineRenderer>();

        if (rope != null) rope.enabled = false;

        // Отключаем физику руки, чтобы она не мешала полету
        if (hand.GetComponent<Rigidbody>())
            hand.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(InputListener());
    }

    void LateUpdate()
    {
        if (isFlying || isAttached)
        {
            UpdateVisuals();
            if (isAttached) ApplyRopeTension();
        }
        else
        {
            ResetGunRotation();
        }
    }

    // Поворот ствола в сторону кисти и отрисовка троса
    void UpdateVisuals()
    {
        if (gunBarrel != null)
            gunBarrel.LookAt(hand.position);

        if (rope != null)
        {
            rope.enabled = true;
            rope.SetPosition(0, startPoint.position);
            rope.SetPosition(1, hand.position);
        }
    }

    void ResetGunRotation()
    {
        if (gunBarrel != null)
            gunBarrel.localRotation = Quaternion.Slerp(gunBarrel.localRotation, Quaternion.identity, Time.deltaTime * 5f);

        if (rope != null) rope.enabled = false;
    }

    IEnumerator InputListener()
    {
        while (true)
        {
            // Выстрел по первому клику
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

        // Фиксируем направление выстрела в момент клика
        Vector3 shootDirection = playerCamera.transform.forward;
        hand.SetParent(null); // Отцепляем руку, чтобы она летела независимо

        // --- ПОЛЕТ ВПЕРЕД ---
        while (Vector3.Distance(startPoint.position, hand.position) < maxDistance)
        {
            Vector3 nextPos = hand.position + shootDirection * speed * Time.deltaTime;

            RaycastHit hit;
            if (Physics.SphereCast(hand.position, collisionRadius, shootDirection, out hit, speed * Time.deltaTime + 0.5f))
            {
                if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                {
                    hand.position = hit.point;
                    hand.forward = hit.normal * -1;
                    yield return StartCoroutine(HandleAttachment(hit));
                    goto ReturnLabel; // Уходим на возврат после отцепления
                }
                else if (hit.collider.transform != playerTransform)
                {
                    break; // Врезались в обычную стену
                }
            }

            hand.position = nextPos;
            hand.forward = shootDirection;
            yield return null;
        }

    ReturnLabel:
        isFlying = true;
        isAttached = false;

        // --- ВОЗВРАТ ---
        while (Vector3.Distance(hand.position, startPoint.position) > 0.3f)
        {
            hand.position = Vector3.MoveTowards(hand.position, startPoint.position, returnSpeed * Time.deltaTime);
            hand.rotation = Quaternion.Slerp(hand.rotation, startPoint.rotation, returnSpeed * 0.2f * Time.deltaTime);
            yield return null;
        }

        ResetHandState();
    }

    IEnumerator HandleAttachment(RaycastHit hit)
    {
        isAttached = true;
        isFlying = false;
        hand.SetParent(hit.transform);

        yield return new WaitForSeconds(0.15f); // Защита от случайного клика

        while (true)
        {
            // Условие отцепления: повторный клик
            if (Input.GetMouseButtonDown(0)) break;

            // Условие отцепления: слишком большая дистанция
            if (Vector3.Distance(playerTransform.position, hand.position) > maxDistance + 5f) break;

            yield return null;
        }

        isAttached = false;
        hand.SetParent(null);
    }

    void ApplyRopeTension()
    {
        float currentDist = Vector3.Distance(playerTransform.position, hand.position);
        if (currentDist > maxDistance)
        {
            Vector3 pullDir = (hand.position - playerTransform.position).normalized;
            playerTransform.position += pullDir * (currentDist - maxDistance);
        }
    }

    void ResetHandState()
    {
        hand.SetParent(originalParent);
        hand.position = startPoint.position;
        hand.rotation = startPoint.rotation;
        isFlying = false;
        isAttached = false;
    }
}
