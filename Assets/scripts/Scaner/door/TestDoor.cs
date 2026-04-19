using UnityEngine;
using System.Collections;

public class TestDoor : MonoBehaviour
{
    public Scanner targetScanner;
    public float OpenHeight = 3f;
    public float Speed = 2f;

    private void OnEnable()
    {
        if (targetScanner != null)
            targetScanner.OnScanComplete += StartOpening;
    }

    private void OnDisable()
    {
        if (targetScanner != null)
            targetScanner.OnScanComplete -= StartOpening;
    }

    private void StartOpening()
    {
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 targetPosition = transform.position + Vector3.up * OpenHeight;

        // Цикл работает только пока дверь движется
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
            yield return null; // Ждем следующего кадра
        }

        transform.position = targetPosition; // Фиксируем финальную точку
        Debug.Log("Дверь открыта, корутина завершена.");
    }
}
