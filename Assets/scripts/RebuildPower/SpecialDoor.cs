using UnityEngine;
using System.Collections;

public class SpecialDoor : MonoBehaviour
{
    // Меняем тип на ScannerPower, чтобы дверь видела наш новый сканер
    public ScannerPower targetScanner;
    public float OpenHeight = 5f; // 18f — это очень высоко, проверь масштаб!
    public float Speed = 2f;

    private bool _isOpening = false;

    // Этот метод мы вызовем напрямую из скрипта ScannerPower
    public void StartOpening()
    {
        if (_isOpening) return; // Чтобы не запускать дважды
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
}
