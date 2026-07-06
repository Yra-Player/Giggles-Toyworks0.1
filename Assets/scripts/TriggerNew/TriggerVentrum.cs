using UnityEngine;

public class TriggerVentrum : MonoBehaviour
{
    [Header("Объект вентиляции")]
    // Сюда перетаскиваем всю вентиляцию (родительский объект)
    [SerializeField] private GameObject ventilationGroup;

    private bool isActivated = false;

    private void Start()
    {
        // Гарантируем, что на старте игры вентиляции нет на сцене
        if (ventilationGroup != null)
        {
            ventilationGroup.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что зашел именно игрок, и активация еще не происходила
        if (!isActivated && other.CompareTag("Player"))
        {
            ActivateVentilation();
        }
    }

    private void ActivateVentilation()
    {
        isActivated = true;

        if (ventilationGroup != null)
        {
            // Включаем вентиляцию на сцене
            ventilationGroup.SetActive(true);
            Debug.Log("Вентиляция успешно активирована на сцене!");
        }
    }
}
