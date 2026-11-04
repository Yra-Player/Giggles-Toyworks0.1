using UnityEngine;

public class TriggerVentrum : MonoBehaviour
{
    [Header("Менеджмент Локаций (Цепочка из 4 шагов)")]

    [Tooltip("1 Переменная: ВКЛЮЧАЕТ саму вентиляцию.")]
    [SerializeField] private GameObject ventilationGroup;

    [Tooltip("2 Переменная: ВКЛЮЧАЕТ условный блок с текстурами без скриптов (заглушку).")]
    [SerializeField] private GameObject staticBlockToEnable;

    [Tooltip("3 Переменная: ВЫКЛЮЧАЕТ локацию лент.")]
    [SerializeField] private GameObject conveyorLocationToDisable;

    [Tooltip("4 Переменная: ВКЛЮЧАЕТ саму финальную комнату.")]
    [SerializeField] private GameObject finalRoomToEnable;

    private bool isActivated = false;

    private void Awake()
    {
        // Самый ранний этап: жестко скрываем локации от процессора
        DeactivateSubLocations();
    }

    private void Start()
    {
        // Повторная подстраховка на первом кадре игры
        DeactivateSubLocations();
    }

    private void DeactivateSubLocations()
    {
        if (ventilationGroup != null && ventilationGroup.activeSelf) ventilationGroup.SetActive(false);
        if (staticBlockToEnable != null && staticBlockToEnable.activeSelf) staticBlockToEnable.SetActive(false);
        if (finalRoomToEnable != null && finalRoomToEnable.activeSelf) finalRoomToEnable.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что зашел именно игрок, и активация еще не происходила
        if (!isActivated && other.CompareTag("Player"))
        {
            ExecuteLocationSwap();
        }
    }

    private void ExecuteLocationSwap()
    {
        isActivated = true;
        Debug.Log($"[Триггер {gameObject.name}]: Игрок зашел. Запуск рокировки из 4 шагов...");

        // 1. Включается сама вентиляция
        if (ventilationGroup != null)
        {
            ventilationGroup.SetActive(true);
            Debug.Log("Шаг 1: Вентиляция успешно активирована!");
        }

        // 2. Включается условный блок с текстурами без скриптов
        if (staticBlockToEnable != null)
        {
            staticBlockToEnable.SetActive(true);
            Debug.Log("Шаг 2: Статический блок текстур включен.");
        }

        // 3. Выключается локация лент
        if (conveyorLocationToDisable != null)
        {
            conveyorLocationToDisable.SetActive(false);
            Debug.Log("Шаг 3: Локация conyeyorLocationToDisable выгружена.");
        }

        // 4. Включается сама финальная комната
        if (finalRoomToEnable != null)
        {
            finalRoomToEnable.SetActive(true);
            Debug.Log("Шаг 4: Финальная комната успешно активирована!");
        }

        Debug.Log("[Триггер]: Все 4 этапа переключения локаций выполнены!");
    }
}
