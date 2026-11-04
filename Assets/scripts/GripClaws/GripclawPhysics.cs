using System.Collections;
using UnityEngine;

//Просчитывает корутины движения и зацепов.

public class GripclawPhysics : MonoBehaviour
{
    private GripclawsManager manager;

    void Awake()
    {
        manager = GetComponent<GripclawsManager>();
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

        while (Vector3.Distance(hand.startPoint.position, hand.handTransform.position) < manager.maxDistance)
        {
            if (!hand.handTransform.gameObject.activeInHierarchy) break;

            Vector3 nextPosition = hand.handTransform.position + shootDirection * manager.speed * Time.deltaTime;
            float stepDistance = manager.speed * Time.deltaTime + 0.5f;
            int layerMask = ~(1 << 2); // Игнорируем Ignore Raycast

            // 1. ПРОВЕРКА НА ТРИГГЕРЫ-СКАНЕРЫ (Специально ищем интерактивные триггеры)
            if (Physics.Raycast(hand.handTransform.position, shootDirection, out RaycastHit triggerHit, stepDistance, layerMask, QueryTriggerInteraction.Collide))
            {
                // Если это триггер, но у него правильный тег (Сканер, Рычаг и т.д.)
                if (triggerHit.collider.isTrigger &&
                    (triggerHit.collider.CompareTag("Scanner") || triggerHit.collider.CompareTag("LeverL") || triggerHit.collider.CompareTag("LadderHandle")))
                {
                    hand.handTransform.position = triggerHit.point;
                    hand.handTransform.forward = triggerHit.normal * -1;
                    yield return StartCoroutine(HandleAttachment(hand, triggerHit));
                    goto ReturnLabel;
                }
            }

            // 2. ПРОВЕРКА НА ТВЕРДЫЕ СТЕНЫ И КОРОБКИ (Игнорируем чекпоинты)
            if (Physics.SphereCast(hand.handTransform.position, manager.collisionRadius, shootDirection, out RaycastHit hit, stepDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform != manager.playerTransform && !hit.collider.transform.IsChildOf(manager.playerTransform))
                {
                    // Твердые объекты (стены, коробки, перекладины)
                    if (hit.collider.CompareTag("Scanner") || hit.collider.CompareTag(manager.draggableTag) ||
                        hit.collider.CompareTag("LeverL") || hit.collider.CompareTag("LadderHandle") ||
                        hit.collider.CompareTag("GripBar"))
                    {
                        hand.handTransform.position = hit.point;
                        hand.handTransform.forward = hit.normal * -1;
                        yield return StartCoroutine(HandleAttachment(hand, hit));
                        goto ReturnLabel;
                    }
                    else break; // Врезались в обычную стену — возвращаем руку
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

        // ВОЗВРАТ СТРОГО ПО ПУТИ НАМОТКИ (Разматывание углов)
        while (hand.bendPoints.Count > 0 || Vector3.Distance(hand.handTransform.position, hand.startPoint.position) > 0.3f)
        {
            if (!hand.gameObject.activeInHierarchy) break;

            // Определяем текущую промежуточную цель для ладони
            // Если есть углы — летим к самому ПОСЛЕДНЕМУ углу. Если углов нет — летим к плечу игрока.
            Vector3 currentTargetPos = hand.bendPoints.Count > 0
                ? hand.bendPoints[hand.bendPoints.Count - 1]
                : hand.startPoint.position;

            // Двигаем руку к текущей цели
            hand.handTransform.position = Vector3.MoveTowards(
                hand.handTransform.position,
                currentTargetPos,
                manager.returnSpeed * Time.deltaTime
            );

            // Плавно разворачиваем руку в сторону ее «гнезда» на плече
            hand.handTransform.rotation = Quaternion.Slerp(
                hand.handTransform.rotation,
                hand.startPoint.rotation,
                manager.returnSpeed * 0.2f * Time.deltaTime
            );

            // Если ладонь вплотную подлетела к текущему углу — удаляем этот угол из списка. 
            // В следующем кадре рука начнет лететь к предыдущему углу или к игроку.
            if (hand.bendPoints.Count > 0 && Vector3.Distance(hand.handTransform.position, currentTargetPos) < 0.4f)
            {
                hand.bendPoints.RemoveAt(hand.bendPoints.Count - 1);
            }

            yield return null;
        }

        // Когда рука окончательно вернулась к плечу
        hand.ResetHandState();
    }

    private IEnumerator HandleAttachment(GripclawHand hand, RaycastHit hit)
    {
        hand.isAttached = true;
        hand.isFlying = false;
        hand.handTransform.SetParent(hit.transform);
        hand.attachedRB = hit.collider.GetComponent<Rigidbody>();

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

            manager.SetDetachCooldown();
            hand.isAttached = false;
            hand.handTransform.SetParent(null);
            yield break;
        }

        LeftLever leftLever = hit.collider.GetComponent<LeftLever>();
        hand.activeInteractable = hit.collider.GetComponent<IGripInteractable>();

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

            float currentDist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
            if (currentDist > manager.maxDistance + 5f || currentDist < 3.2f) break;

            if (Input.GetMouseButton(hand.inputButton))
            {
                pressTimer += Time.deltaTime;
                if (leftLever != null)
                {
                    leftLever.PullLever();
                }
                else if (pressTimer > 0.15f && hand.attachedRB != null)
                {
                    Vector3 dir = hand.startPoint.position - hand.handTransform.position;
                    hand.attachedRB.AddForce(dir.normalized * manager.pullStrength, ForceMode.Acceleration);
                }
            }
            else break;

            yield return null;
        }

        if (leftLever != null) leftLever.ResetLever();
        if (hand.activeInteractable != null) hand.activeInteractable.OnGripStop();

        manager.SetDetachCooldown();
        hand.isAttached = false;
        hand.handTransform.SetParent(null);
    }
}
