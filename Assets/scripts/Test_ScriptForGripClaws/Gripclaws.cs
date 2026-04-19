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

    private Transform originalParent;
    private Quaternion originalRotation;

    // Эти переменные теперь строго контролируют возможность выстрела
    private bool isFlying = false;
    private bool isAttached = false;

    void Start()
    {
        originalParent = hand.parent;
        originalRotation = hand.localRotation;
        if (playerTransform == null) playerTransform = transform;
        if (hand.GetComponent<Rigidbody>()) hand.GetComponent<Rigidbody>().isKinematic = true;

        StartCoroutine(InputListener());
    }

    void Update()
    {
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
            // Проверка: кнопка нажата И рука НЕ в полете И рука НЕ прикреплена
            if (Input.GetMouseButtonDown(0) && !isFlying && !isAttached)
            {
                yield return StartCoroutine(ClawRoutine());
            }
            yield return null;
        }
    }

    IEnumerator ClawRoutine()
    {
        isFlying = true; // Блокируем спам в самом начале
        Vector3 direction = startPoint.forward;

        // --- ВПЕРЕД ---
        while (Input.GetMouseButton(0) && Vector3.Distance(startPoint.position, hand.position) < maxDistance)
        {
            hand.position += direction * speed * Time.deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(hand.position, direction, out hit, 1.5f))
            {
                if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                {
                    isFlying = false; // Полет окончен
                    isAttached = true; // Состояние зацепа
                    hand.position = hit.point;
                    hand.SetParent(hit.transform);

                    // Ждем пока игрок отпустит кнопку
                    while (Input.GetMouseButton(0)) yield return null;

                    isAttached = false; // Отцепились
                    hand.SetParent(originalParent);
                    break;
                }
                else break; // Врезались в стену
            }
            yield return null;
        }

        // --- ВОЗВРАТ ---
        isFlying = true; // Снова полет (возвращение), спам запрещен
        hand.SetParent(originalParent);

        while (Vector3.Distance(hand.position, startPoint.position) > 0.05f || Quaternion.Angle(hand.localRotation, originalRotation) > 0.1f)
        {
            hand.position = Vector3.MoveTowards(hand.position, startPoint.position, speed * Time.deltaTime);
            hand.localRotation = Quaternion.Slerp(hand.localRotation, originalRotation, speed * 0.5f * Time.deltaTime);
            yield return null;
        }

        // Финальная фиксация
        hand.position = startPoint.position;
        hand.localRotation = originalRotation;

        // Разблокировка! Только теперь можно стрелять снова
        isFlying = false;
        isAttached = false;
    }
}
