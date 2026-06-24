using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Настройки направления")]
    [Tooltip("Если включено — везет вперед по стрелке объекта. Если выключено — везет назад.")]
    public bool moveForward = true;

    [Tooltip("Скорость движения ленты")]
    public float speed = 9.0f;

    [Header("Задержка старта")]
    [Tooltip("Задержка в секундах ПОСЛЕ того, как игрок наступил на конвейер")]
    public float startDelay = 3.0f;

    private bool isConveyorActive = false;
    private bool isTriggered = false; // Флаг, чтобы задержка не запускалась повторно

    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    private ConveyorManager manager;

    void Start()
    {
        // Автоматически ищем менеджер конвейеров на сцене при старте игры
        manager = Object.FindFirstObjectByType<ConveyorManager>();
    }

    // Метод для активации конвейера (вызывается менеджером)
    public void ActivateConveyor()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            StartCoroutine(StartConveyorWithDelay());
        }
    }

    private IEnumerator StartConveyorWithDelay()
    {
        Debug.Log($"[Конвейер {gameObject.name}]: Игрок наступил! Запуск таймера задержки...");

        yield return new WaitForSeconds(startDelay);

        isConveyorActive = true;
        Debug.Log($"[Конвейер {gameObject.name}]: Физически запущен!");
    }

    // Физический цикл для плавного перемещения игрока и коробок
    void FixedUpdate()
    {
        if (!isConveyorActive || affectedObjects.Count == 0) return;

        // Вычисляем направление движения относительно вращения самого конвейера
        Vector3 direction = moveForward ? transform.forward : -transform.forward;

        // Двигаем все объекты, которые сейчас лежат на ленте
        for (int i = affectedObjects.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = affectedObjects[i];

            if (rb != null)
            {
                // Принудительно будим объект, если физический движок Unity усыпил его
                if (rb.IsSleeping()) rb.WakeUp();

                // Физически смещаем объект
                Vector3 targetPosition = rb.position + direction * speed * Time.fixedDeltaTime;
                rb.MovePosition(targetPosition);
            }
            else
            {
                // Если объект был уничтожен, удаляем его из списка
                affectedObjects.RemoveAt(i);
            }
        }
    }

    // Событие: кто-то наступил/упал на конвейер
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        // Если у объекта есть Rigidbody, добавляем его в список affectedObjects
        if (rb != null && !affectedObjects.Contains(rb))
        {
            affectedObjects.Add(rb);
        }

        // Если коснулся именно Игрок
        if (collision.gameObject.CompareTag("Player"))
        {
            if (manager != null)
            {
                // Отдаем команду менеджеру запустить ВСЕ конвейеры сразу
                manager.ActivateAllConveyors();
            }
            else
            {
                // Если менеджер на сцене отсутствует, запускаем только этот один конвейер
                ActivateConveyor();
            }
        }
    }

    // Событие: объект сошел или улетел с конвейера
    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && affectedObjects.Contains(rb))
        {
            affectedObjects.Remove(rb);
        }
    }
}
