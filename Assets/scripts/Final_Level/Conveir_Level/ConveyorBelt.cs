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

    [Header("Настройка материала")]
    [Tooltip("Множитель скорости текстуры")]
    public float textureSpeedMultiplier = 0.05f;

    private bool isConveyorActive = false;
    private bool isTriggered = false; // Флаг, чтобы задержка не запускалась повторно

    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    private ConveyorManager manager;

    private Renderer meshRenderer;
    private Vector2 currentTextureOffset = Vector2.zero;

    void Start()
    {
        // Автоматически ищем менеджер конвейеров на сцене при старте игры
        manager = Object.FindFirstObjectByType<ConveyorManager>();

        // Получаем Рендерер, который отвечает за отображение материала
        meshRenderer = GetComponent<Renderer>();

        // ИСПРАВЛЕНО: Ругаемся только если рендерера действительно НЕТ (== null)
        if (meshRenderer == null)
        {
            Debug.LogError($"[Конвейер {gameObject.name}]: На объекте не найден MeshRenderer/SkinnedMeshRenderer! Текстура не будет двигаться.");
        }
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

    void Update()
    {
        // Анимируем текстуру, если лента запущена
        if (!isConveyorActive || meshRenderer == null) return;

        // Вычисление направления UV координат
        float directionSign = moveForward ? -1f : 1f;

        // Считаем смещения на основе скорости конвейера и времени кадра
        float offsetStep = directionSign * speed * textureSpeedMultiplier * Time.deltaTime;

        currentTextureOffset.y += offsetStep;
        meshRenderer.material.mainTextureOffset = currentTextureOffset;
    }

    // Физический цикл для плавного перемещения игрока и коробок
    void FixedUpdate()
    {
        if (!isConveyorActive || affectedObjects.Count == 0) return;

        // Вычисляем направление движения относительно вращения самого конвейера
        Vector3 direction = moveForward ? transform.forward : -transform.forward;

        // ИЗМЕНЕНО: Вместо жесткого расталкивания через MovePosition (которое конфликтует с игроком),
        // задаем объектам скорость (Velocity). Так физика Unity работает гораздо мягче.
        Vector3 targetVelocity = direction * speed;

        for (int i = affectedObjects.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = affectedObjects[i];

            if (rb != null)
            {
                if (rb.IsSleeping()) rb.WakeUp();

                // Сохраняем скорость падения объекта (Y), но принудительно задаем ход конвейера (X и Z)
                Vector3 newVelocity = targetVelocity;
                newVelocity.y = rb.linearVelocity.y;

                rb.linearVelocity = newVelocity;
            }
            else
            {
                affectedObjects.RemoveAt(i);
            }
        }
    }

    // Событие: кто-то наступил/упал на конвейер
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null && !affectedObjects.Contains(rb))
        {
            affectedObjects.Add(rb);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // Находим скрипт движения на игроке и активируем тумблер
            FirstPersonMovement playerMovement = collision.gameObject.GetComponent<FirstPersonMovement>();
            if (playerMovement != null)
            {
                playerMovement.isOnConveyor = true;
            }

            if (manager != null)
            {
                manager.ActivateAllConveyors();
            }
            else
            {
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

        if (collision.gameObject.CompareTag("Player"))
        {
            // Игрок сошёл с конвейера — отключаем тумблер, возвращаем стандартную физику рамп
            FirstPersonMovement playerMovement = collision.gameObject.GetComponent<FirstPersonMovement>();
            if (playerMovement != null)
            {
                playerMovement.isOnConveyor = false;
            }
        }
    }
}
