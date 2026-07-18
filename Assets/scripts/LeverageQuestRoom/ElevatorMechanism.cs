using System.Collections;
using UnityEngine;

public class ElevatorMechanism : MonoBehaviour
{
    [Header("Управляющее устройство (Выбери что-то ОДНО)")]
    [Tooltip("Перетащи сюда рычаг, если вешалкой управляет рычаг")]
    public LeftLever triggerLever;

    [Tooltip("Перетащи сюда магнитофон, если вешалкой управляет магнитофон")]
    public TapeRecorder triggerRecorder;

    [Header("Настройки спуска")]
    [Tooltip("На сколько метров вниз должен опуститься агрегат (например, -3)")]
    public float dropDistance = -3f;
    [Tooltip("Скорость спуска вешалки")]
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(0f, dropDistance, 0f);

        // Подписываемся на событие РЫЧАГА, если он привязан
        if (triggerLever != null)
        {
            triggerLever.OnLeverActivated += StartLowering;
        }

        // Подписываемся на событие МАГНИТОФОНА, если привязан он
        if (triggerRecorder != null)
        {
            triggerRecorder.OnLeverActivated += StartLowering;
        }

        // Защитная проверка, если забыл привязать устройство
        if (triggerLever == null && triggerRecorder == null)
        {
            Debug.LogError($"<color=red>[МЕХАНИЗМ] {gameObject.name} не подключен ни к рычагу, ни к магнитофону!</color>");
        }
    }

    void OnDestroy()
    {
        // Обязательно отписываемся от всего при уничтожении объекта, чтобы Unity не лагала
        if (triggerLever != null)
        {
            triggerLever.OnLeverActivated -= StartLowering;
        }
        if (triggerRecorder != null)
        {
            triggerRecorder.OnLeverActivated -= StartLowering;
        }
    }

    private void StartLowering()
    {
        if (!isMoving)
        {
            StartCoroutine(LoweringRoutine());
        }
    }

    private IEnumerator LoweringRoutine()
    {
        isMoving = true;
        Debug.Log($"<color=cyan>[МЕХАНИЗМ] Начат плавный спуск вешалки {gameObject.name}!</color>");

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        Debug.Log($"<color=green>[МЕХАНИЗМ] Вешалка {gameObject.name} успешно опущена!</color>");
    }
}
