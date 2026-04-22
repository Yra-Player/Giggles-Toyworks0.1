using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Test_Scaner_for_right_hand : MonoBehaviour
{
    [Header("Настройки сканирования")]
    public float RequiredTime = 3f;
    public string PlayerTag = "Right_Arm"; // Изменено на правую руку

    [Header("Настройки интерфейса")]
    public Image progressImage;
    public Color startColor = Color.red;
    public Color endColor = Color.green;

    public event Action OnScanComplete;

    private Coroutine _scanCoroutine;
    private bool _isCompleted = false;

    private void Start()
    {
        if (progressImage != null)
        {
            progressImage.gameObject.SetActive(false);
            progressImage.fillAmount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isCompleted || !other.CompareTag(PlayerTag)) return;

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
        if (other.CompareTag(PlayerTag) && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;

            if (progressImage != null)
            {
                progressImage.gameObject.SetActive(false);
            }

            Debug.Log("Сканирование правой руки прервано");
        }
    }

    private IEnumerator ScanRoutine()
    {
        Debug.Log("Сканирование правой руки...");
        float timer = 0f;

        while (timer < RequiredTime)
        {
            timer += Time.deltaTime;
            float progress = timer / RequiredTime;

            if (progressImage != null)
            {
                progressImage.fillAmount = progress;
                progressImage.color = Color.Lerp(startColor, endColor, progress);
            }

            yield return null;
        }

        _isCompleted = true;
        _scanCoroutine = null;

        if (progressImage != null)
        {
            progressImage.fillAmount = 1f;
            progressImage.color = endColor;
        }

        Debug.Log("Правая рука успешно отсканирована!");
        OnScanComplete?.Invoke();
    }
}

