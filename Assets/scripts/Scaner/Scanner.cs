using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI; // Обязательно для Image

public class Scanner : MonoBehaviour
{
    [Header("Настройки сканирования")]
    public float RequiredTime = 3f;      // Время сканирования
    public string PlayerTag = "Left_Arm"; // Тег руки

    [Header("Настройки интерфейса")]
    public Image progressImage;          // Сюда перетащите объект ProggresBar
    public Color startColor = Color.red;   // Начальный цвет (красный)
    public Color endColor = Color.green;   // Конечный цвет (зеленый)

    public event Action OnScanComplete;

    private Coroutine _scanCoroutine;
    private bool _isCompleted = false;

    private void Start()
    {
        // Изначально скрываем шкалу на сцене
        if (progressImage != null)
        {
            progressImage.gameObject.SetActive(false);
            progressImage.fillAmount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если уже отсканировано или вошел не тот объект — игнорируем
        if (_isCompleted || !other.CompareTag(PlayerTag)) return;

        // Показываем шкалу и сбрасываем её состояние
        if (progressImage != null)
        {
            progressImage.gameObject.SetActive(true);
            progressImage.fillAmount = 0;
            progressImage.color = startColor;
        }

        _scanCoroutine = StartCoroutine(ScanRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        // Если рука ушла до завершения — прерываем
        if (other.CompareTag(PlayerTag) && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;

            // Прячем шкалу обратно
            if (progressImage != null)
            {
                progressImage.gameObject.SetActive(false);
            }

            Debug.Log("Сканирование прервано");
        }
    }

    private IEnumerator ScanRoutine()
    {
        Debug.Log("Сканирование началось...");
        float timer = 0f;

        while (timer < RequiredTime)
        {
            timer += Time.deltaTime;
            float progress = timer / RequiredTime;

            if (progressImage != null)
            {
                progressImage.fillAmount = progress;
                // Плавно меняем цвет от красного к зеленому по мере заполнения
                progressImage.color = Color.Lerp(startColor, endColor, progress);
            }

            yield return null; // Ждем следующего кадра
        }

        // Финальные настройки после завершения
        _isCompleted = true;
        _scanCoroutine = null;

        if (progressImage != null)
        {
            progressImage.fillAmount = 1f;
            progressImage.color = endColor;
            // Если хотите, чтобы шкала исчезла СРАЗУ после успеха, 
            // раскомментируйте строку ниже:
            // progressImage.gameObject.SetActive(false);
        }

        Debug.Log("Сканирование успешно завершено!");
        OnScanComplete?.Invoke();
    }
}
