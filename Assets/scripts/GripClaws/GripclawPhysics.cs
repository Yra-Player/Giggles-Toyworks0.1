using System.Collections;
using UnityEngine;

// Просчитывает корутины движения и зацепов.
public class GripclawPhysics : MonoBehaviour
{
    private GripclawsManager manager;
    private GripclawAudio audioModule;

    void Awake()
    {
        manager = GetComponent<GripclawsManager>();
        audioModule = GetComponent<GripclawAudio>();
    }

    public void LaunchHand(GripclawHand hand)
    {
        StartCoroutine(ClawRoutine(hand));
    }

    private IEnumerator ClawRoutine(GripclawHand hand)
    {
        hand.isFlying = true;
        hand.handTransform.SetParent(null);

        Vector3 shootDirection = manager.playerCamera.transform.forward;
        Ray aimRay = manager.playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(aimRay, out RaycastHit aimHit, manager.maxDistance))
        {
            shootDirection = (aimHit.point - hand.startPoint.position).normalized;
        }
        else
        {
            shootDirection = aimRay.direction;
        }

        while (hand.GetTotalCableLength() < manager.maxDistance)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy) break;

            Vector3 nextPosition = hand.handTransform.position + shootDirection * manager.speed * Time.deltaTime;
            float stepDistance = manager.speed * Time.deltaTime + 0.5f;
            int layerMask = ~(1 << 2);

            // 1. ПРОВЕРКА НА ТРИГГЕРЫ-СКАНЕРЫ И РЫЧАГИ
            if (Physics.Raycast(hand.handTransform.position, shootDirection, out RaycastHit triggerHit, stepDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                if (triggerHit.collider.isTrigger &&
                    (triggerHit.collider.CompareTag("Scanner") || triggerHit.collider.CompareTag("LeverL") || triggerHit.collider.CompareTag("LadderHandle")))
                {
                    hand.handTransform.position = triggerHit.point;
                    hand.handTransform.forward = triggerHit.normal * -1;
                    yield return StartCoroutine(HandleAttachment(hand, triggerHit));
                    goto ReturnLabel;
                }
            }

            // 2. ПРОВЕРКА НА ТВЕРДЫЕ СТЕНЫ И КОРОБКИ
            if (Physics.SphereCast(hand.handTransform.position, manager.collisionRadius, shootDirection, out RaycastHit hit, stepDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform != manager.playerTransform && !hit.collider.transform.IsChildOf(manager.playerTransform))
                {
                    if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag(manager.draggableTag) ||
                        hit.collider.CompareTag("LeverL") || hit.collider.CompareTag("LadderHandle") ||
                        hit.collider.CompareTag("GripBar"))
                    {
                        hand.handTransform.position = hit.point;
                        hand.handTransform.forward = hit.normal * -1;
                        yield return StartCoroutine(HandleAttachment(hand, hit));
                        goto ReturnLabel;
                    }
                    else
                    {
                        if (audioModule != null) audioModule.PlayDetachAudio(hand);
                        break;
                    }
                }
            }

            hand.handTransform.position = nextPosition;
            hand.handTransform.forward = shootDirection;
            yield return null;
        }

    ReturnLabel:
        hand.isFlying = true;
        hand.isAttached = false;
        hand.handTransform.SetParent(null);

        if (audioModule != null) audioModule.PlayReturnAudio(hand);

        // ВОЗВРАТ СТРОГО ПО ПУТИ НАМОТКИ (Разматывание углов)
        while (hand.bendPoints.Count > 0 || Vector3.Distance(hand.handTransform.position, hand.startPoint.position) > 0.3f)
        {
            if (!hand.gameObject.activeInHierarchy) break;

            Vector3 currentTargetPos = hand.bendPoints.Count > 0
                ? hand.bendPoints[hand.bendPoints.Count - 1]
                : hand.startPoint.position;

            hand.handTransform.position = Vector3.MoveTowards(
                hand.handTransform.position,
                currentTargetPos,
                manager.returnSpeed * Time.deltaTime
            );

            hand.handTransform.rotation = Quaternion.Slerp(
                hand.handTransform.rotation,
                hand.startPoint.rotation,
                manager.returnSpeed * 0.2f * Time.deltaTime
            );

            if (hand.bendPoints.Count > 0 && Vector3.Distance(hand.handTransform.position, currentTargetPos) < 0.4f)
            {
                hand.bendPoints.RemoveAt(hand.bendPoints.Count - 1);
            }

            yield return null;
        }

        if (audioModule != null) audioModule.StopAudio(hand);
        hand.ResetHandState();
    }

    private IEnumerator HandleAttachment(GripclawHand hand, RaycastHit hit)
    {
        hand.isAttached = true;
        hand.isFlying = false;
        hand.handTransform.SetParent(hit.transform);
        hand.attachedRB = hit.collider.GetComponent<Rigidbody>();

        // Идеальное магнитное выравнивание лапки по центру сканера (ищем в родителе, если коллайдер на дочернем объекте)
        if (hit.collider.CompareTag("Scanner"))
        {
            Scanner scanner = hit.collider.GetComponentInParent<Scanner>();
            if (scanner != null && scanner.handSnapPoint != null)
            {
                hand.handTransform.position = scanner.handSnapPoint.position;
                hand.handTransform.rotation = scanner.handSnapPoint.rotation;
            }
        }

        if (audioModule != null) audioModule.PlayAttachAudio(hand);

        if (hit.collider.CompareTag("GripBar"))
        {
            FirstPersonMovement movement = manager.playerTransform.GetComponent<FirstPersonMovement>();
            if (movement != null)
            {
                Vector3 grabPoint = hit.point;
                Vector3 hangPosition = grabPoint + Vector3.down * 12.5f;

                yield return new WaitForSeconds(manager.hitStickDuration);

                bool hasArrived = false;
                float swingTimer = 0f;

                while (Input.GetMouseButton(hand.inputButton) && hit.collider != null)
                {
                    if (!hasArrived)
                    {
                        movement.StartPulling(hangPosition, 14f);
                        if (Vector3.Distance(manager.playerTransform.position, hangPosition) < 0.3f) hasArrived = true;
                    }
                    else
                    {
                        swingTimer += Time.deltaTime * 3f;
                        Vector3 swingOffset = new Vector3(Mathf.Sin(swingTimer) * 0.15f, Mathf.Cos(swingTimer * 2f) * 0.05f, 0f);
                        Vector3 dynamicHangPos = hangPosition + manager.playerTransform.rotation * swingOffset;

                        movement.StartPulling(dynamicHangPos, 5f);

                        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f) break;
                    }
                    yield return null;
                }
                movement.StopPulling();
            }

            if (audioModule != null) audioModule.PlayDetachAudio(hand);
            manager.SetDetachCooldown();
            hand.isAttached = false;
            hand.handTransform.SetParent(null);
            yield break;
        }

        LeftLever leftLever = hit.collider.GetComponentInParent<LeftLever>();
        hand.activeInteractable = hit.collider.GetComponentInParent<IGripInteractable>();

        if (hand.activeInteractable != null) hand.activeInteractable.OnGripStart(hand.inputButton);

        float stickTimer = 0f;
        while (stickTimer < manager.hitStickDuration)
        {
            stickTimer += Time.deltaTime;
            if (Input.GetMouseButton(hand.inputButton)) break;
            yield return null;
        }

        float pressTimer = 0;
        while (hand.isAttached)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy || hit.collider == null) break;

            // Пока игрок зажимает кнопку — рука держится до последнего
            if (!Input.GetMouseButton(hand.inputButton)) break;

            float directDist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
            if (hand.bendPoints.Count == 0 && directDist < 3.2f) break;

            pressTimer += Time.deltaTime;
            if (leftLever != null)
            {
                leftLever.PullLever();
            }
            else if (pressTimer > 0.15f && hand.attachedRB != null)
            {
                Vector3 pullTarget = hand.bendPoints.Count > 0
                    ? hand.bendPoints[hand.bendPoints.Count - 1]
                    : hand.startPoint.position;

                Vector3 dir = pullTarget - hand.handTransform.position;
                hand.attachedRB.AddForce(dir.normalized * manager.pullStrength, ForceMode.Acceleration);
            }

            yield return null;
        }

        if (leftLever != null) leftLever.ResetLever();
        if (hand.activeInteractable != null) hand.activeInteractable.OnGripStop();

        if (audioModule != null) audioModule.PlayDetachAudio(hand);

        manager.SetDetachCooldown();
        hand.isAttached = false;
        hand.handTransform.SetParent(null);
    }
}
