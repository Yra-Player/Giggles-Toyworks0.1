using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{

    public Transform DoorOpenTransform;
    public Transform DoorCloseTransform;
    private float OpenHeight = 20f;
    public float Speed = 2f;
    public float SpeedClose = 7f;

    private bool isOpened = false;

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !isOpened)
        {
            if (DoorOpenTransform != null)
            {
                isOpened = true;
                StartCoroutine(OpenCloserDoor());
            }

        }
        if (other.CompareTag("Player") && isOpened)
        {
            if (DoorCloseTransform != null)
            {
                isOpened = false;
                StartCoroutine(CloseDoor());
            }

        }
    }

    private IEnumerator OpenCloserDoor()
    {

        Vector3 targetPosition = DoorOpenTransform.position + Vector3.up * OpenHeight;

        while (Vector3.Distance(DoorOpenTransform.position, targetPosition) > 0.01f)
        {
            DoorOpenTransform.position = Vector3.MoveTowards(DoorOpenTransform.position, targetPosition, Speed * Time.deltaTime);
            yield return null;
        }

        DoorOpenTransform.position = targetPosition;
        Debug.Log("Дверь открыта. Может за ней есть кто живой?");
    }

    private IEnumerator CloseDoor()
    {

        Vector3 targetPosition = DoorCloseTransform.position + Vector3.down * OpenHeight;

        while (Vector3.Distance(DoorCloseTransform.position, targetPosition) > 0.01f)
        {
            DoorCloseTransform.position = Vector3.MoveTowards(DoorCloseTransform.position, targetPosition, SpeedClose * Time.deltaTime);
            yield return null;
        }

        DoorCloseTransform.position = targetPosition;
        Debug.Log("Дверь закрыта.Ты в безопасности, но надолго ли?");
    }
}
