using System.Collections;
using UnityEngine;

public class JammedDoorOptimized : MonoBehaviour
{
    [Header("Настройки времени (в секундах)")]
    [Tooltip("Сколько ворота стоят вверху")]
    public float topPauseDuration = 1.0f;
    [Tooltip("Сколько ворота стоят внизу")]
    public float bottomPauseDuration = 0.5f;

    [Header("Настройки движения")]
    [Tooltip("Высота, на которую ворота пытаются приподняться")]
    public float liftHeight = 0.4f;
    [Tooltip("Скорость подъема ворот вверх")]
    public float riseSpeed = 2.0f;
    [Tooltip("Скорость падения ворот вниз (сделайте ее больше, например 8-10)")]
    public float fallSpeed = 8.0f;

    private void Start()
    {
        // Запускаем оптимизированный цикл
        StartCoroutine(JammedCycleRoutine());
    }

    private IEnumerator JammedCycleRoutine()
    {
        Vector3 bottomPosition = transform.position;
        Vector3 topPosition = bottomPosition + Vector3.up * liftHeight;

        // Кэшируем задержки для оптимизации памяти
        WaitForSeconds waitAtTop = new WaitForSeconds(topPauseDuration);
        WaitForSeconds waitAtBottom = new WaitForSeconds(bottomPauseDuration);

        while (true)
        {
            // 1. ДВИЖЕНИЕ ВВЕРХ (Медленный подъем)
            while (Vector3.Distance(transform.position, topPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, topPosition, riseSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = topPosition;

            // 2. ПАУЗА ВВЕРХУ
            yield return waitAtTop;

            // 3. ДВИЖЕНИЕ ВНИЗ (Быстрое падение)
            while (Vector3.Distance(transform.position, bottomPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, bottomPosition, fallSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = bottomPosition;

            // 4. ПАУЗА ВНИЗУ
            yield return waitAtBottom;
        }
    }
}
