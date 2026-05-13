using UnityEngine;

public class LeftLever : MonoBehaviour
{
    [Header("Настройки вращения")]
    public Transform leverHandle;
    public float targetAngle = -60f;
    public float rotationSpeed = 5f;

    private float currentAngle = 0f;
    private bool isFullyActivated = false;

    void Start()
    {
        if (leverHandle == null) leverHandle = this.transform;
    }

    public void PullLever()
    {
        if (isFullyActivated) return;

        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime * 50f);
        leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

        if (Mathf.Approximately(currentAngle, targetAngle))
        {
            isFullyActivated = true;
            Debug.Log("<color=green>ЛЕВЫЙ РЫЧАГ ОПУЩЕН ДО УПОРА!</color>");
        }
    }

    public void ResetLever()
    {
        if (isFullyActivated) return;

        currentAngle = Mathf.MoveTowards(currentAngle, 0f, rotationSpeed * Time.deltaTime * 50f);
        leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public bool IsActivated() => isFullyActivated;
}
