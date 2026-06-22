using System.Collections;
using UnityEngine;

public class TriggerOpenVentrumHatch : MonoBehaviour
{
    [Header("Настройка двери")]
    [SerializeField] private Transform hatchTransform;
    [SerializeField] private float openHeight = 12.55f;
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float delayBeforOpen = 1.5f;

    private Vector3 closePosition;
    private Vector3 openPosition;
    private Coroutine movementCoroutine;

    private void Start()
    {
        if (hatchTransform == null)
        {
            hatchTransform = transform;
        }

        closePosition = hatchTransform.position;
        openPosition = closePosition + Vector3.up * openHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveHatchRoutine(openPosition, "Дверь начинает открываться..."));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveHatchRoutine(closePosition, "Дверь начинает закрываться..."));
        }
    }

    private IEnumerator MoveHatchRoutine(Vector3 targetPosition, string debugMessage)
    {
        yield return new WaitForSeconds(delayBeforOpen);

        Debug.Log(debugMessage);

        while (hatchTransform.position != targetPosition)
        {
            hatchTransform.position = Vector3.MoveTowards(
                hatchTransform.position,
                targetPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }
}
