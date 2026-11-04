using System.Collections.Generic;
using UnityEngine;

public class GripclawHand : MonoBehaviour
{
    [Header("Hand Setup")]
    public string handName = "Hand";
    public int inputButton = 0;
    public Transform handTransform;
    public Transform startPoint;
    public LineRenderer rope;
    public Transform ropeCylinder;

    [Header("Audio Setup")]
    public AudioSource audioSource;
    public AudioClip flySound;
    public AudioClip returnSound;
    public AudioClip attachSound;
    public AudioClip detachSound;

    [HideInInspector] public List<Vector3> bendPoints = new List<Vector3>();
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public bool isFlying = false;
    [HideInInspector] public bool isAttached = false;
    [HideInInspector] public Rigidbody attachedRB;
    [HideInInspector] public IGripInteractable activeInteractable;

    void Start()
    {
        if (handTransform == null) handTransform = transform;
        originalParent = handTransform.parent;

        if (rope != null) rope.enabled = false;

        Rigidbody rb = handTransform.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public void ResetHandState()
    {
        if (rope != null) rope.enabled = false;
        bendPoints.Clear();

        if (originalParent != null)
            handTransform.SetParent(originalParent);
        else
            handTransform.SetParent(transform);

        handTransform.position = startPoint.position;
        handTransform.rotation = startPoint.rotation;

        isFlying = false;
        isAttached = false;
        attachedRB = null;
        activeInteractable = null;

        if (ropeCylinder != null)
        {
            Vector3 cScale = ropeCylinder.localScale;
            cScale.y = 0f;
            ropeCylinder.localScale = cScale;
        }
    }
}
