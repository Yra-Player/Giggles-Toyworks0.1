using System.Collections;
using UnityEngine;

public class GripclawsManager : MonoBehaviour
{
    [System.Serializable]
    public class HandData
    {
        public string name = "Hand";
        public Transform handTransform;
        public Transform startPoint;
        public LineRenderer rope;
        public int inputButton;
        [HideInInspector] public bool isFlying, isAttached, isReturning;
    }

    [Header("Setup")]
    public HandData[] hands;
    public Camera playerCamera;
    public float speed = 30f;
    public float returnSpeed = 60f;
    public float maxDistance = 40f;

    [Header("Layers")]
    public LayerMask obstacleMask; // Стены (чтобы рука не летела сквозь них)
    public LayerMask grabMask;     // Сканеры

    void Update()
    {
        foreach (var hand in hands)
        {
            if (Input.GetMouseButtonDown(hand.inputButton) && !hand.isFlying && !hand.isAttached && !hand.isReturning)
            {
                StartCoroutine(ClawRoutine(hand));
            }
        }
    }

    void LateUpdate()
    {
        foreach (var hand in hands)
        {
            if (hand.isFlying || hand.isAttached || hand.isReturning)
            {
                DrawSimpleRope(hand);
            }
        }
    }

    IEnumerator ClawRoutine(HandData hand)
    {
        hand.isFlying = true;
        Vector3 dir = playerCamera.transform.forward;
        hand.handTransform.SetParent(null);
        hand.handTransform.localScale = Vector3.one;

        // --- ПОЛЕТ ВПЕРЕД ---
        while (Vector3.Distance(hand.startPoint.position, hand.handTransform.position) < maxDistance && !hand.isAttached)
        {
            hand.handTransform.position += dir * speed * Time.deltaTime;
            hand.handTransform.forward = dir;

            if (Physics.SphereCast(hand.handTransform.position, 0.2f, dir, out RaycastHit hit, 0.5f, obstacleMask | grabMask))
            {
                if (((1 << hit.collider.gameObject.layer) & grabMask) != 0)
                {
                    if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag("In") || hit.collider.CompareTag("Out"))
                    {
                        AttachHand(hand, hit);
                        break;
                    }
                }
                break; // Просто врезались в стену
            }
            yield return null;
        }

        // --- ОЖИДАНИЕ (Если зацепились) ---
        while (hand.isAttached)
        {
            if (Input.GetMouseButtonDown(hand.inputButton)) break;
            yield return null;
        }

        // --- ВОЗВРАТ (Прямой) ---
        hand.isAttached = false;
        hand.isReturning = true;
        hand.handTransform.SetParent(null);

        while (Vector3.Distance(hand.handTransform.position, hand.startPoint.position) > 0.3f)
        {
            hand.handTransform.position = Vector3.MoveTowards(hand.handTransform.position, hand.startPoint.position, returnSpeed * Time.deltaTime);
            hand.handTransform.rotation = Quaternion.Slerp(hand.handTransform.rotation, hand.startPoint.rotation, 10f * Time.deltaTime);
            hand.handTransform.localScale = Vector3.one;
            yield return null;
        }

        ResetHand(hand);
    }

    void AttachHand(HandData hand, RaycastHit hit)
    {
        hand.isAttached = true;
        hand.isFlying = false;
        hand.handTransform.position = hit.point;
        hand.handTransform.forward = hit.normal * -1;
        hand.handTransform.SetParent(hit.transform);

        // Нейтрализация масштаба родителя
        Vector3 ps = hit.transform.lossyScale;
        hand.handTransform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
    }

    void DrawSimpleRope(HandData hand)
    {
        if (hand.rope == null) return;
        hand.rope.enabled = true;
        hand.rope.positionCount = 2;
        hand.rope.SetPosition(0, hand.startPoint.position);
        hand.rope.SetPosition(1, hand.handTransform.position);
    }

    void ResetHand(HandData hand)
    {
        hand.isFlying = hand.isAttached = hand.isReturning = false;
        hand.handTransform.SetParent(hand.startPoint.parent);
        hand.handTransform.position = hand.startPoint.position;
        hand.handTransform.rotation = hand.startPoint.rotation;
        hand.handTransform.localScale = Vector3.one;
        hand.rope.enabled = false;
    }
}
