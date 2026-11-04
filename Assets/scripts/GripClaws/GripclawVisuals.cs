using System.Collections.Generic;
using UnityEngine;

public class GripclawVisuals : MonoBehaviour
{
    private GripclawsManager manager;
    private Dictionary<GripclawHand, List<Transform>> cylinderPool = new Dictionary<GripclawHand, List<Transform>>();

    void Awake()
    {
        manager = GetComponent<GripclawsManager>();
    }

    public void RenderHandVisuals(GripclawHand[] hands)
    {
        foreach (var hand in hands)
        {
            if (hand == null) continue;

            if (hand.handTransform != null && (hand.isFlying || hand.isAttached))
            {
                if (hand.rope != null) hand.rope.enabled = true;

                DrawRope(hand);
                UpdateCableBending(hand);
                UpdateSegmentedCylinders(hand);
            }
            else
            {
                if (hand.rope != null) hand.rope.enabled = false;
                HideAllCylinders(hand);
            }
        }
    }

    private void DrawRope(GripclawHand hand)
    {
        if (hand.rope == null) return;

        hand.rope.startWidth = 0.05f;
        hand.rope.endWidth = 0.05f;

        int pointCount = 2 + hand.bendPoints.Count;
        hand.rope.positionCount = pointCount;
        hand.rope.SetPosition(0, hand.startPoint.position);

        for (int i = 0; i < hand.bendPoints.Count; i++)
        {
            hand.rope.SetPosition(i + 1, hand.bendPoints[i]);
        }
        hand.rope.SetPosition(pointCount - 1, hand.handTransform.position);
    }

    private void UpdateCableBending(GripclawHand hand)
    {
        if (!hand.isAttached && !hand.isFlying) return;
        if (hand.handTransform == null) return;

        int layerMask = ~(1 << 2);

        Vector3 lastPoint = hand.bendPoints.Count > 0 ? hand.bendPoints[hand.bendPoints.Count - 1] : hand.startPoint.position;
        Vector3 handPos = hand.handTransform.position;

        if (Vector3.Distance(lastPoint, handPos) > 0.05f)
        {
            if (Physics.Linecast(lastPoint, handPos, out RaycastHit hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != manager.playerTransform && hit.transform != hand.handTransform && !hit.collider.isTrigger)
                {
                    Vector3 offsetPos = hit.point + hit.normal * 0.25f;
                    if (hand.bendPoints.Count == 0 || Vector3.Distance(hand.bendPoints[hand.bendPoints.Count - 1], offsetPos) > 0.4f)
                    {
                        hand.bendPoints.Add(offsetPos);
                        return;
                    }
                }
            }
        }

        Vector3 firstPoint = hand.bendPoints.Count > 0 ? hand.bendPoints[0] : hand.handTransform.position;
        Vector3 playerPos = hand.startPoint.position;

        if (Vector3.Distance(playerPos, firstPoint) > 0.05f)
        {
            if (Physics.Linecast(playerPos, firstPoint, out RaycastHit hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != manager.playerTransform && hit.transform != hand.handTransform && !hit.collider.isTrigger)
                {
                    Vector3 offsetPos = hit.point + hit.normal * 0.25f;
                    if (hand.bendPoints.Count == 0 || Vector3.Distance(hand.bendPoints[0], offsetPos) > 0.4f)
                    {
                        hand.bendPoints.Insert(0, offsetPos);
                        return;
                    }
                }
            }
        }

        if (hand.bendPoints.Count > 0)
        {
            Vector3 pointBeforeLast = hand.bendPoints.Count > 1 ? hand.bendPoints[hand.bendPoints.Count - 2] : hand.startPoint.position;
            if (!Physics.Linecast(pointBeforeLast, handPos, out RaycastHit hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                hand.bendPoints.RemoveAt(hand.bendPoints.Count - 1);
                return;
            }

            if (hand.bendPoints.Count > 0)
            {
                Vector3 pointAfterFirst = hand.bendPoints.Count > 1 ? hand.bendPoints[1] : hand.handTransform.position;
                if (!Physics.Linecast(playerPos, pointAfterFirst, out hit, layerMask, QueryTriggerInteraction.Ignore))
                {
                    hand.bendPoints.RemoveAt(0);
                    return;
                }
            }
        }
    }

    private void UpdateSegmentedCylinders(GripclawHand hand)
    {
        if (hand.ropeCylinder == null || hand.startPoint == null) return;

        List<Vector3> allPoints = new List<Vector3>();
        allPoints.Add(hand.startPoint.position);
        allPoints.AddRange(hand.bendPoints);
        allPoints.Add(hand.handTransform.position);

        int neededSegments = allPoints.Count - 1;

        if (!cylinderPool.ContainsKey(hand))
        {
            cylinderPool[hand] = new List<Transform>();
            cylinderPool[hand].Add(hand.ropeCylinder);
        }

        List<Transform> currentPool = cylinderPool[hand];

        while (currentPool.Count < neededSegments)
        {
            Transform spawned = Instantiate(hand.ropeCylinder, hand.ropeCylinder.parent);
            spawned.name = hand.ropeCylinder.name + "_Segment_" + currentPool.Count;
            currentPool.Add(spawned);
        }

        for (int i = 0; i < currentPool.Count; i++)
        {
            Transform segment = currentPool[i];

            if (i < neededSegments)
            {
                segment.gameObject.SetActive(true);

                Vector3 pStart = allPoints[i];
                Vector3 pEnd = allPoints[i + 1];
                Vector3 vector = pEnd - pStart;
                float distance = vector.magnitude;

                segment.position = pStart + (vector.normalized * (distance * 0.5f));

                if (distance > 0.001f)
                {
                    segment.rotation = Quaternion.LookRotation(vector) * Quaternion.Euler(90f, 0f, 0f);
                }

                float baseScaleY = distance / 2f;
                float parentLossyScaleY = segment.parent != null ? segment.parent.lossyScale.y : 1f;
                if (parentLossyScaleY < 0.001f) parentLossyScaleY = 1f;

                Vector3 newScale = segment.localScale;
                newScale.y = baseScaleY / parentLossyScaleY;
                segment.localScale = newScale;
            }
            else
            {
                if (segment != hand.ropeCylinder)
                {
                    segment.gameObject.SetActive(false);
                }
                else
                {
                    Vector3 zeroScale = segment.localScale;
                    zeroScale.y = 0f;
                    segment.localScale = zeroScale;
                }
            }
        }
    }

    private void HideAllCylinders(GripclawHand hand)
    {
        if (hand.ropeCylinder == null) return;

        hand.ropeCylinder.localPosition = Vector3.zero;
        hand.ropeCylinder.localRotation = Quaternion.Slerp(hand.ropeCylinder.localRotation, Quaternion.identity, 12f * Time.deltaTime);

        Vector3 currentScale = hand.ropeCylinder.localScale;
        currentScale.y = Mathf.MoveTowards(currentScale.y, 0f, 15f * Time.deltaTime);
        hand.ropeCylinder.localScale = currentScale;

        if (cylinderPool.ContainsKey(hand))
        {
            foreach (Transform segment in cylinderPool[hand])
            {
                if (segment != hand.ropeCylinder && segment != null)
                {
                    segment.gameObject.SetActive(false);
                }
            }
        }
    }
}
