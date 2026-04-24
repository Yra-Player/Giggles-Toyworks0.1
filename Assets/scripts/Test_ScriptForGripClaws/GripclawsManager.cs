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
        [HideInInspector] public bool wasActiveLastFrame;
    }

    [Header("Setup")]
    public HandData[] hands;
    public Camera playerCamera;
    public float speed = 30f;
    public float returnSpeed = 60f;
    public float maxDistance = 40f;

    [Header("Layers")]
    public LayerMask obstacleMask;
    public LayerMask grabMask;

    void Update()
    {
        foreach (var hand in hands)
        {
            if (hand.handTransform == null) continue;

            // Проверка на активацию (подбор руки)
            if (hand.handTransform.gameObject.activeInHierarchy && !hand.wasActiveLastFrame)
            {
                ResetHand(hand);
            }
            hand.wasActiveLastFrame = hand.handTransform.gameObject.activeInHierarchy;

            // Если рука выключена - игнорируем ввод
            if (!hand.handTransform.gameObject.activeInHierarchy) continue;

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
            if (hand.handTransform == null || !hand.handTransform.gameObject.activeInHierarchy)
            {
                if (hand.rope != null) hand.rope.enabled = false;
                continue;
            }

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
            if (!hand.handTransform.gameObject.activeInHierarchy) { ResetHand(hand); yield break; }

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
                break;
            }
            yield return null;
        }

        // --- ОЖИДАНИЕ ---
        while (hand.isAttached)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy) { ResetHand(hand); yield break; }
            if (Input.GetMouseButtonDown(hand.inputButton)) break;
            yield return null;
        }

        // --- ВОЗВРАТ ---
        hand.isAttached = false;
        hand.isReturning = true;
        hand.handTransform.SetParent(null);

        while (Vector3.Distance(hand.handTransform.position, hand.startPoint.position) > 0.3f)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy) { ResetHand(hand); yield break; }

            hand.handTransform.position = Vector3.MoveTowards(hand.handTransform.position, hand.startPoint.position, returnSpeed * Time.deltaTime);
            hand.handTransform.rotation = Quaternion.Slerp(hand.handTransform.rotation, hand.startPoint.rotation, 10f * Time.deltaTime);
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

        // Прямой возврат в иерархию, как в твоем первом коде
        if (hand.handTransform != null && hand.startPoint != null)
        {
            hand.handTransform.SetParent(hand.startPoint.parent);
            hand.handTransform.position = hand.startPoint.position;
            hand.handTransform.rotation = hand.startPoint.rotation;
            hand.handTransform.localScale = Vector3.one;
        }

        // Тот самый фикс ошибки NullReference
        if (hand.rope != null)
        {
            hand.rope.enabled = false;
        }
    }
}
