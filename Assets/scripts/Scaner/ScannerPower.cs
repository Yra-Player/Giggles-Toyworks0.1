using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScannerPower : MonoBehaviour
{
    [Header("Настройки времени")]
    public float totalScanTime = 3f;      
    public float timeUntilFailure = 1f;  
    public string playerTag = "Left_Arm"; 

    [Header("Интерфейс")]
    public Image progressImage;
    public Color startColor = Color.red;
    public Color endColor = Color.green;

    [Header("Визуал сканера")]
    public Renderer scannerModelRenderer;
    public Color scannerActiveColor = Color.blue;
    public Color scannerBrokenColor = Color.red;
    public Color scannerSuccessColor = Color.green;
    public SpecialDoor doorScript;

    private bool _isBroken = false;
    private bool _receivingPower = false; // Флаг получения энергии
    private Coroutine _scanCoroutine;
    private bool _isPermanentlyPowered = false;

    // --- ПУБЛИЧНЫЕ МЕТОДЫ (Взаимодействие) ---

    // Этот метод вызывает PowerBox через корутину
    public void ReceiveExternalPower()
    {
        _receivingPower = true;
    }

    public void SetPermanentPower()
    {
        _isPermanentlyPowered = true;
        Debug.Log("Сканер: Питание зациклено, я больше не сломаюсь!");
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
        Debug.Log("Система перезагружена, сканер готов!");
    }

    // --- ЛОГИКА UNITY ---

    private void Start()
    {
        if (progressImage != null) progressImage.gameObject.SetActive(false);
        if (scannerModelRenderer != null) scannerModelRenderer.material.color = scannerActiveColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isBroken || !other.CompareTag(playerTag)) return;

        if (progressImage != null)
        {
            progressImage.gameObject.SetActive(true);
            progressImage.fillAmount = 0;
            progressImage.color = startColor;
        }

        if (_scanCoroutine != null) StopCoroutine(_scanCoroutine);
        _scanCoroutine = StartCoroutine(SmartScanRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        // Если сканер не сломан, прячем полоску при уходе руки
        if (!_isBroken && other.CompareTag(playerTag) && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;
            if (progressImage != null) progressImage.gameObject.SetActive(false);
        }
    }

    // --- КОРУТИНА СКАНИРОВАНИЯ ---

    private IEnumerator SmartScanRoutine()
    {
        float timer = 0f;

        while (timer < totalScanTime)
        {

            if (timer >= timeUntilFailure && !_receivingPower && !_isPermanentlyPowered)
            {
                BreakSystem();
                yield break;
            }

            timer += Time.deltaTime;
            float progress = timer / totalScanTime;

            if (progressImage != null)
            {
                progressImage.fillAmount = progress;
                progressImage.color = Color.Lerp(startColor, endColor, progress);
            }

            // ВАЖНО: Сбрасываем флаг каждый кадр. 
            // Если PowerBox не вызовет ReceiveExternalPower на следующем кадре, питание пропадет.
            _receivingPower = false;

            yield return null;
        }

        Success();
    }

    private void BreakSystem()
    {
        _isBroken = true;
        if (scannerModelRenderer != null) scannerModelRenderer.material.color = scannerBrokenColor;
        if (progressImage != null) progressImage.color = Color.red;

        Debug.Log("⚠️ КРИТИЧЕСКАЯ ОШИБКА: Питание прервано на " + timeUntilFailure + " сек.");
        _scanCoroutine = null;
    }

    private void Success()
    {
        if (scannerModelRenderer != null) scannerModelRenderer.material.color = scannerSuccessColor;
        Debug.Log("✅ СКАНИРОВАНИЕ ЗАВЕРШЕНО УСПЕШНО!");
        if (doorScript != null) doorScript.StartOpening();
        _scanCoroutine = null;
    }
}
