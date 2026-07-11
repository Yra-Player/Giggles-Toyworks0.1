using UnityEngine;
using System.Collections;

public class SpecialDoor : MonoBehaviour
{
    public ScannerPower targetScanner;
    public float OpenHeight = 5f;
    public float Speed = 2f;

    private bool _isOpening = false;
    private string uniqueID; // Уникальный ID объекта для сохранения

    private void Start()
    {
        // Генерируем уникальное имя на основе имени объекта на сцене
        uniqueID = "SpecialDoor_" + gameObject.name + "_" + transform.position.ToString();

        // Проверяем: если в PlayerPrefs записано, что дверь уже открывалась
        if (LevelStateManager.GetState(uniqueID) == 1)
        {
            // Мгновенно перемещаем её в открытое положение без анимации
            transform.position = transform.position + Vector3.up * OpenHeight;
            _isOpening = true;
            Debug.Log($"[SpecialDoor] {gameObject.name} автоматически восстановлена в ОТКРЫТОМ состоянии.");
        }
    }

    public void StartOpening()
    {
        if (_isOpening) return;

        // Сохраняем состояние "Открыто" (1) в момент активации
        LevelStateManager.SaveState(uniqueID, 1);

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        _isOpening = true;
        Vector3 targetPosition = transform.position + Vector3.up * OpenHeight;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log("Дверь открыта, питание пазла завершено успешно!");
    }

    public void StartClosing()
    {
        // Метод-заглушка для триггера компиляции
    }
}
