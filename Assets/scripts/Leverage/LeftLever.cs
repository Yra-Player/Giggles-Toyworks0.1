using System.Collections;
using UnityEngine;

public class LeftLever : MonoBehaviour
{
    [Header("Настройки вращения")]
    public Transform leverHandle;
    public float targetAngle = -60f;
    public float rotationSpeed = 5f;

    private float currentAngle = 0f;
    private bool isFullyActivated = false;

    private Coroutine pullCoroutine;
    private Coroutine resetCoroutine;

    [HideInInspector] public System.Action<LeftLever> OnLeverActivated;

    void Start()
    {
        if (leverHandle == null) leverHandle = this.transform;
    }

    public void PullLever()
    {
        if (isFullyActivated) return;


        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (pullCoroutine != null) return;

        pullCoroutine = StartCoroutine(PullRoutine());
    }

    public void ResetLever()
    {
        if (isFullyActivated) return;

        if (pullCoroutine != null)
        {
            StopCoroutine(pullCoroutine);
            pullCoroutine = null;
        }

        if (resetCoroutine == null)
        {
            resetCoroutine = StartCoroutine(ResetRoutine());
        }
    }

    private IEnumerator PullRoutine()
    {
        while (Mathf.Abs(currentAngle - targetAngle) > 0.5f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotationSpeed * Time.deltaTime * 50f);
            leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }
        currentAngle = targetAngle;
        leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        isFullyActivated = true;
        pullCoroutine = null;

        Debug.Log($"<color=green>[РЫЧАГ] {gameObject.name} ОПУЩЕН ДО УПОРА!</color>");

        HydraulicPuzzleManager manager = FindObjectOfType<HydraulicPuzzleManager>();
        if (manager != null)
        {
            manager.CheckPuzzleConditionManual();
        }

        OnLeverActivated?.Invoke(this);
    }

    private IEnumerator ResetRoutine()
    {
        while (!Mathf.Approximately(currentAngle, 0f))
        {
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, rotationSpeed * Time.deltaTime * 50f);
            leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        currentAngle = 0f;
        leverHandle.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        resetCoroutine = null;
    }

    public bool IsActivated() => isFullyActivated;
}
