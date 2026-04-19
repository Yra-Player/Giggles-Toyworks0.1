using UnityEngine;
using System;
using System.Collections;

public class Scaner : MonoBehaviour
{
    public float RequiredTime = 3f;
    public event Action OnScanComplete;

    private Coroutine _scanCoroutine;
    private bool _isCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        // Если уже отсканировано или зашел не тот объект — игнорируем
        if (_isCompleted || !other.CompareTag("Left_Arm")) return;

        // Запускаем процесс сканирования
        _scanCoroutine = StartCoroutine(ScanRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Left_Arm") && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;
            Debug.Log("Сканирование прервано");
        }
    }

    private IEnumerator ScanRoutine()
    {
        Debug.Log("Сканирование началось...");

        // Просто ждем нужное количество секунд
        yield return new WaitForSeconds(RequiredTime);

        _isCompleted = true;
        _scanCoroutine = null;

        Debug.Log("Сканирование успешно завершено!");
        OnScanComplete?.Invoke();
    }
}
