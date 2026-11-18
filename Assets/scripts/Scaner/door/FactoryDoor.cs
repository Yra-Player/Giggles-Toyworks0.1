using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FactoryDoor : MonoBehaviour
{
    public enum DoorMovementType
    {
        Slide,  // Сдвиг по прямой (вверх, влево, вправо — идеально для фабричных ворот)
        Rotate  // Поворот вокруг оси (как обычная дверь на петлях)
    }

    [Header("Настройки движения двери")]
    [Tooltip("Тип движения: сдвиг или поворот")]
    public DoorMovementType movementType = DoorMovementType.Slide;

    [Tooltip("Смещение для Slide (например, Y = 4 для подъема вверх) или углы для Rotate (например, Y = 90)")]
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);

    [Tooltip("Время полного открытия/закрытия в секундах")]
    public float duration = 2f;

    [Tooltip("Задержка перед началом движения после сигнала сканера")]
    public float startDelay = 0f;

    [Header("Аудио")]
    public AudioSource audioSource;
    public AudioClip moveLoopClip;   // Звук мотора / скрежета при движении
    public AudioClip finishHitClip;  // Глухой удар, когда ворота доехали до упора

    [Header("Дополнительные события")]
    public UnityEvent onDoorOpened;  // Сработает, когда дверь полностью откроется

    private Vector3 _closedLocalPos;
    private Quaternion _closedLocalRot;
    private Vector3 _openLocalPos;
    private Quaternion _openLocalRot;

    private bool _isOpen = false;
    private Coroutine _moveCoroutine;

    private void Awake()
    {
        // Запоминаем закрытое положение на старте
        _closedLocalPos = transform.localPosition;
        _closedLocalRot = transform.localRotation;

        // Вычисляем целевое открытое положение
        _openLocalPos = _closedLocalPos + openOffset;
        _openLocalRot = _closedLocalRot * Quaternion.Euler(openOffset);
    }

    // Этот метод мы привяжем в Инспекторе к событию сканера (OnScanCompleteUnity)
    public void OpenDoor()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveDoorRoutine(_openLocalPos, _openLocalRot, true));
    }

    // На будущее: пригодится, если дверь нужно будет захлопнуть за игроком триггером
    public void CloseDoor()
    {
        if (!_isOpen) return;
        _isOpen = false;

        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveDoorRoutine(_closedLocalPos, _closedLocalRot, false));
    }

    private IEnumerator MoveDoorRoutine(Vector3 targetPos, Quaternion targetRot, bool opening)
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        if (audioSource != null && moveLoopClip != null)
        {
            audioSource.clip = moveLoopClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float rawProgress = Mathf.Clamp01(elapsed / duration);

            // Плавный разгон и плавное торможение в конце (SmoothStep)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, rawProgress);

            if (movementType == DoorMovementType.Slide)
            {
                transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothProgress);
            }
            else
            {
                transform.localRotation = Quaternion.Slerp(startRot, targetRot, smoothProgress);
            }

            yield return null;
        }

        // Жестко фиксируем финальную точку
        if (movementType == DoorMovementType.Slide)
            transform.localPosition = targetPos;
        else
            transform.localRotation = targetRot;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (finishHitClip != null) audioSource.PlayOneShot(finishHitClip);
        }

        _moveCoroutine = null;

        if (opening)
        {
            onDoorOpened?.Invoke();
        }
    }
}
