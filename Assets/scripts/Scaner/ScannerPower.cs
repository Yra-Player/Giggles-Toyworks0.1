using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScannerPower : MonoBehaviour
{
    [Header("Настройки времени")]
    public float totalScanTime = 3f;      // Сколько сканирование длилось БЫ в идеале
    public float timeUntilFailure = 1f;  // Точный момент обрыва (1 секунда)
    public string PlayerTag = "Left_Arm";

    [Header("Интерфейс")]
    public Image progressImage;
    public Color startColor = Color.red;
    public Color endColor = Color.green;

    [Header("Визуал сканера")]
    public Renderer scannerModelRenderer;
    public Color scannerActiveColor = Color.blue;
    public Color scannerBrokenColor = Color.red;

    private bool _isBroken = false;
    private Coroutine _scanCoroutine;

    private void Start()
    {
        if (progressImage != null) progressImage.gameObject.SetActive(false);
        if (scannerModelRenderer != null) scannerModelRenderer.material.color = scannerActiveColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isBroken || !other.CompareTag(PlayerTag)) return;

        if (progressImage != null)
        {
            progressImage.gameObject.SetActive(true);
            progressImage.fillAmount = 0;
            progressImage.color = startColor;
        }
        _scanCoroutine = StartCoroutine(BrokenScanRoutine());
    }

    private IEnumerator BrokenScanRoutine()
    {
        float timer = 0f;

        // Цикл работает только до момента поломки
        while (timer < timeUntilFailure)
        {
            timer += Time.deltaTime;
            float progress = timer / totalScanTime; // Прогресс считаем от общего времени

            if (progressImage != null)
            {
                progressImage.fillAmount = progress;
                progressImage.color = Color.Lerp(startColor, endColor, progress);
            }
            yield return null;
        }

        // --- МОМЕНТ ПОЛОМКИ ---
        _isBroken = true;

        // Меняем цвет сканера на красный (сломан)
        if (scannerModelRenderer != null)
            scannerModelRenderer.material.color = scannerBrokenColor;

        // Опционально: можно сделать цвет полоски серым или ярко-красным в момент замирания
        if (progressImage != null) progressImage.color = Color.red;

        Debug.Log("СИСТЕМНАЯ ОШИБКА: Сканирование прервано на " + timeUntilFailure + " сек.");

        // Корутина просто заканчивается, оставляя progressImage.fillAmount в текущем положении
        _scanCoroutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        // Если сканер УЖЕ сломался, мы НЕ прячем полоску, она остается "зависшей" на экране
        if (!_isBroken && other.CompareTag(PlayerTag) && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            if (progressImage != null) progressImage.gameObject.SetActive(false);
        }
    }
    public void Restore()
    {
        _isBroken = false;
        if (scannerModelRenderer != null)
            scannerModelRenderer.material.color = scannerActiveColor;

        if (progressImage != null)
        {
            progressImage.fillAmount = 0;
            progressImage.gameObject.SetActive(false);
        }
        Debug.Log("Питание восстановлено, сканер готов к работе!");
    }
}
