using System.Collections;
using UnityEngine;

public class Gripclaws : MonoBehaviour
{
    [Header("Links")]
    public Transform hand;
    public Transform startPoint;
    public Transform playerTransform;
    public Camera playerCamera; // Добавьте ссылку на камеру в инспекторе

    [Header("Settings")]
    public float speed = 30f;
    public float maxDistance = 40f;

    private Transform originalParent;
    private Quaternion originalRotation;
    private bool isFlying = false;
    private bool isAttached = false;

    void Start()
    {
        originalParent = hand.parent;
        originalRotation = hand.localRotation;

        if (playerTransform == null) playerTransform = transform;
        if (playerCamera == null) playerCamera = Camera.main; // Авто-поиск камеры

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

        // 1. Определяем точку, в которую летим
        Vector3 targetPoint;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Луч из центра экрана
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, maxDistance))
        {
            targetPoint = hitInfo.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        // Вычисляем направление полета от startPoint к найденной точке
        Vector3 flyDirection = (targetPoint - startPoint.position).normalized;

        // --- ВПЕРЕД ---
        // Летим, пока зажата кнопка И не достигли цели/лимита
        while (Input.GetMouseButton(0) && Vector3.Distance(startPoint.position, hand.position) < maxDistance)
        {
            hand.position += flyDirection * speed * Time.deltaTime;

            // Поворачиваем клешню по направлению полета для визуала
            hand.forward = flyDirection;

            RaycastHit hit;
            // Проверка столкновения во время полета
            if (Physics.Raycast(hand.position, flyDirection, out hit, 1.0f))
            {
                if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("Interactive"))
                {
                    isFlying = false;
                    isAttached = true;
                    hand.position = hit.point;
                    hand.SetParent(hit.transform);

                    while (Input.GetMouseButton(0)) yield return null;

                    isAttached = false;
                    hand.SetParent(originalParent);
                    break;
                }
                else
                {
                    // Если врезались в обычную стену — возвращаемся
                    break;
                }
            }

            // Если почти долетели до targetPoint, тоже останавливаемся
            if (Vector3.Distance(hand.position, targetPoint) < 0.5f) break;

            yield return null;
        }

        // --- ВОЗВРАТ ---
        isFlying = true;
        hand.SetParent(originalParent);

        while (Vector3.Distance(hand.position, startPoint.position) > 0.1f)
        {
            hand.position = Vector3.MoveTowards(hand.position, startPoint.position, speed * Time.deltaTime);
            // Плавное возвращение вращения в исходное состояние
            hand.localRotation = Quaternion.Slerp(hand.localRotation, originalRotation, speed * 0.5f * Time.deltaTime);
            yield return null;
        }

        hand.position = startPoint.position;
        hand.localRotation = originalRotation;
        isFlying = false;
        isAttached = false;
    }
}
