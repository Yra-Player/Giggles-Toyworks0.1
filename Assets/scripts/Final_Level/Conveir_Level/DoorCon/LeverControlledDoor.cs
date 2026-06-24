using UnityEngine;
using System.Collections;

public class LeverControlledDoor : MonoBehaviour
{
    [Header("Связь с рычагом")]
    public LeftLever lever;

    [Header("Настройка движения")]
    public float openSpeed = 2f;
    public float targetHeight = 4f;

    private void Start()
    {
        if (lever != null)
        {
            lever.OnLeverActivated += StartOpening;
        }
        else
        {
            Debug.LogError($"[ВОРОТА] На объекте {gameObject.name} не назначен рычаг!");
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от события при удалении объекта для защиты от утечек памяти
        if (lever != null)
        {
            lever.OnLeverActivated -= StartOpening;
        }
    }

    private void StartOpening()
    {
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 targetPosition = transform.position + Vector3.up * targetHeight;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
            yield return null; 
        }

        transform.position = targetPosition;
        Debug.Log($"<color=cyan>[ВОРОТА] {gameObject.name} ОТКРЫЛИСЬ!</color>");
    }
}
