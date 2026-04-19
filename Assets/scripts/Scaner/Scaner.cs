using UnityEngine;
using UnityEngine.UI; // Обязательно добавьте это для работы с Image
using System;
using System.Collections;

public class Scanner : MonoBehaviour
{
    [Header("Settings")]
    public float RequiredTime = 3f;
    public string TargetTag = "Left_Arm";

    [Header("UI")]
    public Image progressBar; // Перетащите сюда вашу картинку (Fill Image)

    public event Action OnScanComplete;

    private bool _isCompleted = false;
    private Coroutine _scanCoroutine;

    private void Start()
    {
        if (progressBar != null) progressBar.fillAmount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isCompleted && other.CompareTag(TargetTag))
        {
            if (_scanCoroutine == null)
                _scanCoroutine = StartCoroutine(ScanRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TargetTag))
            StopScanning();
    }

    private IEnumerator ScanRoutine()
    {
        float timer = 0f;

        while (timer < RequiredTime)
        {
            timer += Time.deltaTime;

            // Обновляем шкалу: текущее время делим на нужное время (получаем от 0 до 1)
            if (progressBar != null)
                progressBar.fillAmount = timer / RequiredTime;

            yield return null;
        }

        _isCompleted = true;
        if (progressBar != null) progressBar.fillAmount = 1;
        OnScanComplete?.Invoke();
        if (progressBar != null) progressBar.color = Color.green;

    }

    private void StopScanning()
    {
        // Если сканирование уже УСПЕШНО завершено, выходим из метода и ничего не сбрасываем
        if (_isCompleted) return;

        if (_scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;

            // Сбрасываем шкалу, только если сканирование было ПРЕРВАНО до конца
            if (progressBar != null) progressBar.fillAmount = 0;
            Debug.Log(">>> СКАНЕР: Рука убрана до завершения, прогресс сброшен.");
        }
    }
}
