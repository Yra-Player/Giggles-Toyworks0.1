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

    private Renderer meshRenderer;
    private Vector2 currentTextureOffset = Vector2.zero;

    void Start()
    {
        // Получаем Рендерер, который отвечает за отображение материала
        meshRenderer = GetComponent<Renderer>();

        if (meshRenderer == null)
        {
            Debug.LogError($"[Конвейер {gameObject.name}]: На объекте не найден MeshRenderer! Текстура не будет двигаться.");
        }
    }

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

        // Передаем скорость игроку ТОЛЬКО ПОСЛЕ того, как таймер кончился и лента поехала
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            // Проверяем, находится ли он всё еще на ленте
            if (playerRb != null && affectedObjects.Contains(playerRb))
            {
                FirstPersonMovement playerMovement = player.GetComponent<FirstPersonMovement>();
                if (playerMovement != null)
                {
                    playerMovement.isOnConveyor = true;
                    Vector3 direction = moveForward ? transform.forward : -transform.forward;
                    playerMovement.activeConveyorVelocity = direction * speed;
                }
            }
        }

        Debug.Log($"[Конвейер {gameObject.name}]: Физически запущен!");
    }

    void Update()
    {
        // Анимируем текстуру, если лента запущена
        if (!isConveyorActive || meshRenderer == null) return;

        // ИСПРАВЛЕНО: Синхронизировали знаки направления визуала с физикой хода
        float directionSign = moveForward ? 1f : -1f;

        // Считаем смещения на основе скорости конвейера и времени кадра
        float offsetStep = directionSign * speed * textureSpeedMultiplier * Time.deltaTime;

        currentTextureOffset.y += offsetStep;
        meshRenderer.material.mainTextureOffset = currentTextureOffset;
    }

    void FixedUpdate()
    {
        if (!isConveyorActive || affectedObjects.Count == 0) return;

        Vector3 direction = moveForward ? transform.forward : -transform.forward;
        Vector3 conveyorVelocity = direction * speed;

        for (int i = affectedObjects.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = affectedObjects[i];

            if (rb != null)
            {
                if (rb.IsSleeping()) rb.WakeUp();

                // Игрока мы здесь больше не двигаем принудительно, его логика теперь внутри FirstPersonMovement
                if (!rb.CompareTag("Player"))
                {
                    // Для обычных коробок и прочих физических объектов
                    Vector3 newVelocity = conveyorVelocity;
                    newVelocity.y = rb.linearVelocity.y;
                    rb.linearVelocity = newVelocity;
                }
            }
            else
            {
                affectedObjects.RemoveAt(i);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null && !affectedObjects.Contains(rb))
        {
            affectedObjects.Add(rb);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // Если конвейер ВПЕРВЫЕ учуял игрока — запускаем его с задержкой
            if (!isTriggered)
            {
                ActivateConveyor();
            }
            
            else if (isConveyorActive)
            {
                FirstPersonMovement playerMovement = collision.gameObject.GetComponent<FirstPersonMovement>();
                if (playerMovement != null)
                {
                    playerMovement.isOnConveyor = true;
                    Vector3 direction = moveForward ? transform.forward : -transform.forward;
                    playerMovement.activeConveyorVelocity = direction * speed;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && affectedObjects.Contains(rb))
        {
            affectedObjects.Remove(rb);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            FirstPersonMovement playerMovement = collision.gameObject.GetComponent<FirstPersonMovement>();
            if (playerMovement != null)
            {
                playerMovement.isOnConveyor = false;
                playerMovement.activeConveyorVelocity = Vector3.zero;
            }
        }
    }
}
