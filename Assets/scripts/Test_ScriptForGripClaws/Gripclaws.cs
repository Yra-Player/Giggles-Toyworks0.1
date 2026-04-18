using System.Collections;
using UnityEngine;

public class Gripclaws : MonoBehaviour
{
    [Header("Links")]
    public Transform hand;
    public Transform startPoint;
    public Transform playerTransform;

    [Header("Settings")]
    public float speed = 30f;
    public float maxDistance = 40f;
    public float scannerHoldTime = 3f;

    private Transform originalParent;
    private Quaternion originalRotation; // Сохраняем исходный поворот
    private bool isFlying = false;
    private bool isAttached = false;

    void Start()
    {
        originalParent = hand.parent;
        // Запоминаем локальный поворот руки в спокойном состоянии
        originalRotation = hand.localRotation;

        if (playerTransform == null) playerTransform = transform;
        if (hand.GetComponent<Rigidbody>()) hand.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(InputListener());
    }

    void Update()
    {
        // Ограничение дистанции игрока
        if (isAttached)
        {
            float currentDist = Vector3.Distance(playerTransform.position, hand.position);
            if (currentDist > maxDistance)
            {
                Vector3 directionToHand = (hand.position - playerTransform.position).normalized;
                float overshoot = currentDist - maxDistance;
                playerTransform.position += directionToHand * overshoot;
            }
        }
    }

    IEnumerator InputListener()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !isFlying && !isAttached)
            {
                StartCoroutine(ClawRoutine());
            }
            yield return null;
        }
    }

    IEnumerator ClawRoutine()
    {
        isFlying = true;
        Vector3 direction = startPoint.forward;

        // --- ВПЕРЕД ---
        while (Input.GetMouseButton(0) && Vector3.Distance(startPoint.position, hand.position) < maxDistance)
        {
            hand.position += direction * speed * Time.deltaTime;

            if (Physics.Raycast(hand.position, direction, out RaycastHit hit, 0.8f))
            {
                if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                {
                    isFlying = false;
                    isAttached = true;
                    hand.position = hit.point;
                    hand.SetParent(hit.transform);

                    if (hit.collider.CompareTag("Scanner"))
                    {
                        float timer = 0;
                        while (timer < scannerHoldTime && Input.GetMouseButton(0))
                        {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                    }
                    else
                    {
                        while (Input.GetMouseButton(0)) yield return null;
                    }

                    isAttached = false;
                    hand.SetParent(originalParent);
                    break;
                }
                else break;
            }
            yield return null;
        }

        // --- ВОЗВРАТ (Позиция + Вращение) ---
        isFlying = true;
        hand.SetParent(originalParent);

        // Пока не вернемся И по позиции, И по вращению
        while (Vector3.Distance(hand.position, startPoint.position) > 0.05f ||
               Quaternion.Angle(hand.localRotation, originalRotation) > 0.1f)
        {
            // Плавно возвращаем позицию
            hand.position = Vector3.MoveTowards(hand.position, startPoint.position, speed * Time.deltaTime);

            // Плавно возвращаем вращение в исходное локальное состояние
            hand.localRotation = Quaternion.Slerp(hand.localRotation, originalRotation, speed * 0.5f * Time.deltaTime);

            yield return null;
        }

        // Финальная жесткая установка для точности
        hand.position = startPoint.position;
        hand.localRotation = originalRotation;

        isFlying = false;
        isAttached = false;
    }
}
