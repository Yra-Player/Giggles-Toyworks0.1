using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Scanner : MonoBehaviour, IGripInteractable
{
    public enum AllowedHandType
    {
        LeftHandOnly = 0,   // Только левая рука (ЛКМ / 0)
        RightHandOnly = 1,  // Только правая рука (ПКМ / 1)
        AnyHand = 2         // Любая рука
    }

    [Header("Настройки сканера")]
    public AllowedHandType allowedHand = AllowedHandType.LeftHandOnly;
    public Transform handSnapPoint;
    public float requiredTime = 3f;
    public float drainSpeed = 1.5f;

    [Header("Сбой питания (Головоломка со щитком)")]
    [Tooltip("Включать ТОЛЬКО на том сканере, который должен выбить свет")]
    public bool tripPowerOnFirstUse = false;
    public float tripAfterSeconds = 1f;
    public Color offlineColor = new Color(0.05f, 0.05f, 0.05f);
    public UnityEvent onPowerTripUnity;
    public UnityEvent onPowerRestoredUnity;

    [Header("3D-шкала из кубиков")]
    public Renderer[] progressCubes;

    [Header("Цвета индикации")]
    public Color idleHandColor = new Color(0f, 0.4f, 1f);
    public Color activeFillColor = Color.green;
    public bool useEmission = true;
    public float emissionIntensity = 2f;

    [Header("Аудио")]
    public AudioSource audioSource;
    public AudioClip scanningLoopClip;
    public AudioClip completeClip;
    public AudioClip cancelClip;
    public AudioClip powerTripClip;

    [Header("События при завершении")]
    public UnityEvent onScanCompleteUnity;
    public event Action OnScanComplete;

    private float _currentProgress = 0f;
    private float _holdTimer = 0f;
    private bool _isHolding = false;
    private bool _isCompleted = false;
    private bool _hasPower = true;
    private bool _powerAlreadyTripped = false;

    private Coroutine _processCoroutine;
    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        UpdateCubesVisual();
    }

    public void OnGripStart(int handInputButton)
    {
        if (_isCompleted || !_hasPower)
        {
            if (!_hasPower && audioSource != null && cancelClip != null)
            {
                audioSource.PlayOneShot(cancelClip);
            }
            return;
        }

        if (allowedHand != AllowedHandType.AnyHand && (int)allowedHand != handInputButton)
        {
            if (audioSource != null && cancelClip != null)
            {
                audioSource.PlayOneShot(cancelClip);
            }
            return;
        }

        _isHolding = true;
        _holdTimer = 0f;

        if (audioSource != null && scanningLoopClip != null)
        {
            audioSource.clip = scanningLoopClip;
            audioSource.loop = true;
            audioSource.pitch = 0.85f;
            audioSource.Play();
        }

        if (_processCoroutine == null)
        {
            _processCoroutine = StartCoroutine(ScanAndDrainRoutine());
        }
    }

    public void OnGripStop()
    {
        if (_isCompleted || !_isHolding) return;

        _isHolding = false;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.pitch = 1f;
            if (cancelClip != null) audioSource.PlayOneShot(cancelClip);
        }
    }

    private IEnumerator ScanAndDrainRoutine()
    {
        while (!_isCompleted && _hasPower)
        {
            if (_isHolding)
            {
                _holdTimer += Time.deltaTime;
                _currentProgress += Time.deltaTime / requiredTime;

                if (audioSource != null && scanningLoopClip != null)
                {
                    audioSource.pitch = Mathf.Lerp(0.85f, 1.45f, _currentProgress);
                }

                if (tripPowerOnFirstUse && !_powerAlreadyTripped && _holdTimer >= tripAfterSeconds)
                {
                    TriggerPowerTrip();
                    yield break;
                }

                if (_currentProgress >= 1f)
                {
                    CompleteScan();
                    yield break;
                }
            }
            else
            {
                _currentProgress -= Time.deltaTime * drainSpeed;

                if (_currentProgress <= 0f)
                {
                    _currentProgress = 0f;
                    UpdateCubesVisual();
                    _processCoroutine = null;
                    yield break;
                }
            }

            UpdateCubesVisual();
            yield return null;
        }
    }

    private void TriggerPowerTrip()
    {
        _powerAlreadyTripped = true;
        _hasPower = false;
        _isHolding = false;
        _currentProgress = 0f;
        _processCoroutine = null;

        UpdateCubesVisual();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.pitch = 1f;
            if (powerTripClip != null) audioSource.PlayOneShot(powerTripClip);
        }

        Debug.Log($"[Scanner: {gameObject.name}] Питание выбито!");
        onPowerTripUnity?.Invoke();
    }

    public void RestorePower()
    {
        _hasPower = true;
        _powerAlreadyTripped = true;
        _currentProgress = 0f;

        UpdateCubesVisual();
        Debug.Log($"[Scanner: {gameObject.name}] Питание восстановлено! Кубики обновлены.");
        onPowerRestoredUnity?.Invoke();
    }

    private void UpdateCubesVisual()
    {
        if (progressCubes == null || progressCubes.Length == 0) return;

        int activeCount = Mathf.FloorToInt(_currentProgress * progressCubes.Length);
        if (_isCompleted) activeCount = progressCubes.Length;

        for (int i = 0; i < progressCubes.Length; i++)
        {
            Renderer cubeRenderer = progressCubes[i];
            if (cubeRenderer == null) continue;

            Color targetColor;
            Color emissionColor;

            if (!_hasPower)
            {
                targetColor = offlineColor;
                // Используем минимальный почти черный цвет вместо Color.black, 
                // чтобы URP не отключал поддержку Emission навсегда
                emissionColor = new Color(0.002f, 0.002f, 0.002f);
            }
            else
            {
                bool isFilled = i < activeCount;
                targetColor = isFilled ? activeFillColor : idleHandColor;
                emissionColor = isFilled ? (activeFillColor * emissionIntensity) : (idleHandColor * 0.4f);
            }

            cubeRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", targetColor);
            _propBlock.SetColor("_Color", targetColor);

            if (useEmission)
            {
                cubeRenderer.sharedMaterial.EnableKeyword("_EMISSION");
                _propBlock.SetColor("_EmissionColor", emissionColor);
            }

            cubeRenderer.SetPropertyBlock(_propBlock);
        }
    }

    private void CompleteScan()
    {
        _currentProgress = 1f;
        _isHolding = false;
        _isCompleted = true;
        _processCoroutine = null;

        UpdateCubesVisual();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.pitch = 1f;
            if (completeClip != null) audioSource.PlayOneShot(completeClip);
        }

        Debug.Log($"[Scanner: {gameObject.name}] Сканирование завершено на 100%! Открываем дверь.");
        OnScanComplete?.Invoke();
        onScanCompleteUnity?.Invoke();
    }
}
