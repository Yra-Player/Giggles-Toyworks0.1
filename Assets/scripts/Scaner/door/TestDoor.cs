using UnityEngine;
using System.Collections;

public class TestDoor : MonoBehaviour
{
    public Scanner targetScanner;
    private float OpenHeight = 18f;
    public float Speed = 2f;

    private Vector3 initialPosition; // —тартова€ позици€ (закрыто)
    private Vector3 targetPosition;  //  онечна€ позици€ (открыто)
    private Coroutine currentDoorRoutine;

    private void Start()
    {
        initialPosition = transform.position;
        // «аранее рассчитываем точку полного открыти€
        targetPosition = initialPosition + Vector3.up * OpenHeight;
    }

    private void OnEnable()
    {
        if (targetScanner != null)
            targetScanner.OnScanComplete += StartOpening;
    }

    private void OnDisable()
    {
        if (targetScanner != null)
            targetScanner.OnScanComplete -= StartOpening;
    }

    public void StartOpening()
    {
        // ќстанавливаем любое текущее движение (например, закрытие)
        if (currentDoorRoutine != null) StopCoroutine(currentDoorRoutine);

        // «апускаем движение к targetPosition из текущего положени€
        currentDoorRoutine = StartCoroutine(MoveDoorRoutine(targetPosition, "открыта"));
    }

    public void StartClosing()
    {
        // ќстанавливаем любое текущее движение (например, открытие)
        if (currentDoorRoutine != null) StopCoroutine(currentDoorRoutine);

        // «апускаем движение к initialPosition из текущего положени€
        currentDoorRoutine = StartCoroutine(MoveDoorRoutine(initialPosition, "закрыта"));
    }

    // ”ниверсальна€ корутина дл€ движени€ в любую сторону
    private IEnumerator MoveDoorRoutine(Vector3 destination, string debugState)
    {
        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Speed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        Debug.Log($"ƒверь {debugState}, корутина завершена.");
    }
}
