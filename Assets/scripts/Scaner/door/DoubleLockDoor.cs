using UnityEngine;
using System.Collections;

public class DoubleLockDoor : MonoBehaviour
{
    [Header("Сканеры")]
    public Scanner leftScanner;          // Ссылка на первый сканер (левый)
    public Test_Scaner_for_right_hand rightScanner; // Ссылка на второй сканер (правый)

    [Header("Настройки двери")]
    public float OpenHeight = 3f;
    public float Speed = 2f;

    private bool _isLeftReady = false;
    private bool _isRightReady = false;
    private bool _isOpened = false;

    private void OnEnable()
    {
        // Подписываемся на оба сканера
        if (leftScanner != null)
            leftScanner.OnScanComplete += HandleLeftScan;

        if (rightScanner != null)
            rightScanner.OnScanComplete += HandleRightScan;
    }

    private void OnDisable()
    {
        // Отписываемся, чтобы избежать ошибок
        if (leftScanner != null)
            leftScanner.OnScanComplete -= HandleLeftScan;

        if (rightScanner != null)
            rightScanner.OnScanComplete -= HandleRightScan;
    }

    private void HandleLeftScan()
    {
        _isLeftReady = true;
        Debug.Log("Левый сканер активирован");
        CheckBothScanners();
    }

    private void HandleRightScan()
    {
        _isRightReady = true;
        Debug.Log("Правый сканер активирован");
        CheckBothScanners();
    }

    private void CheckBothScanners()
    {
        // Если оба сканера сработали и дверь еще не открыта
        if (_isLeftReady && _isRightReady && !_isOpened)
        {
            _isOpened = true;
            StartOpening();
        }
    }

    private void StartOpening()
    {
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Debug.Log("Оба сканера подтверждены! Открываю дверь...");
        Vector3 targetPosition = transform.position + Vector3.up * OpenHeight;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log("Дверь полностью открыта.");
    }
}
