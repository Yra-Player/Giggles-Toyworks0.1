using System.Collections;
using UnityEngine;

public class CeilingHatch : MonoBehaviour, IGripInteractable
{
    [Header("Ссылки")]
    public Transform hatchTransform;
    public Rigidbody ladderRigidbody; 

    [Header("Настройки анимации люка")]
    public float openAngle = -90f;
    public float openSpeed = 5f;
    public Vector3 rotationAxis = Vector3.forward; 

    [Header("Тайминги")]
    public float holdDuration = 3f;

    private bool isActivated = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        if (hatchTransform != null)
        {
            initialRotation = hatchTransform.localRotation;
            targetRotation = initialRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        }

        
        if (ladderRigidbody != null)
        {
            ladderRigidbody.isKinematic = true;
        }
    }

    public void OnGripStart(int mouseButton)
    {
        if (!isActivated)
        {
            isActivated = true;
            StartCoroutine(HatchSequenceRoutine());
        }
    }

    public void OnGripStop()
    {
    }

    IEnumerator HatchSequenceRoutine()
    {
        yield return new WaitForSeconds(holdDuration);

        
        if (ladderRigidbody != null)
        {
            ladderRigidbody.isKinematic = false;
        }

        while (hatchTransform != null && Quaternion.Angle(hatchTransform.localRotation, targetRotation) > 0.1f)
        {
            hatchTransform.localRotation = Quaternion.Slerp(hatchTransform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        if (hatchTransform != null)
        {
            hatchTransform.localRotation = targetRotation;
        }
    }
}
