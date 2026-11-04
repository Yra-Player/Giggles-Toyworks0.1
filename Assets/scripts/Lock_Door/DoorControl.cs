using System.Collections;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public enum KeyType { StaffOnly, MiniMarket }

    [Header("Настройки доступа")]
    public bool isLocked = true;
    public KeyType requiredKey;

    [Header("Анимация")]
    public Vector3 openRotationOffset = new Vector3(0, 90, 0);
    public float speed = 2f;
    public bool autoClose = true;
    public float closeDelay = 3f;

    private bool isOpen = false;
    private bool isProcess = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(openRotationOffset);
    }

    public void TryOpen(Inventory inventory)
    {
        if (isProcess) return;

        if (isLocked)
        {
            if (requiredKey == KeyType.StaffOnly && inventory.HasStaffKey())
            {
                inventory.UseStaffKey();
                UnlockAndOpen();
            }
            else if (requiredKey == KeyType.MiniMarket && inventory.HasMarketKey())
            {
                inventory.UseMarketKey();
                UnlockAndOpen();
            }
            else
            {
                StartCoroutine(ShakeDoor());
            }
        }
        else
        {
            StartCoroutine(MoveDoor(!isOpen));
        }
    }

    // ИНТЕГРАЦИЯ: Этот метод будет вызывать кнопка с пульта охраны
    public void InteractFromPanel()
    {
        if (isProcess) return;

        // Если дверь была заперта на ключ, пульт её взламывает/отпирает
        if (isLocked)
        {
            UnlockAndOpen();
        }
        else
        {
            // Если уже отперта — просто открывает или закрывает
            StartCoroutine(MoveDoor(!isOpen));
        }
    }

    void UnlockAndOpen()
    {
        isLocked = false;
        StartCoroutine(MoveDoor(true));
    }

    IEnumerator MoveDoor(bool targetOpen)
    {
        isProcess = true;
        Quaternion start = transform.localRotation;
        Quaternion end = targetOpen ? openRot : closedRot;
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Slerp(start, end, progress);
            yield return null;
        }
        isOpen = targetOpen;
        isProcess = false;
        if (isOpen && autoClose) StartCoroutine(AutoCloseTimer());
    }

    IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(closeDelay);
        if (isOpen && !isProcess) StartCoroutine(MoveDoor(false));
    }

    IEnumerator ShakeDoor()
    {
        isProcess = true;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            float z = Mathf.Sin(Time.time * 50f) * 2f;
            transform.localRotation = closedRot * Quaternion.Euler(0, z, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = closedRot;
        isProcess = false;
    }
}
