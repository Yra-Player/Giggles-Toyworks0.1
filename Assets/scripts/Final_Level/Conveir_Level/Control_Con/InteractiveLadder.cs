using System.Collections;
using UnityEngine;

public class InteractiveLadder : MonoBehaviour, IGripInteractable
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда главный родительский объект лестницы, который должен крутиться")]
    public Transform ladderRoot;

    [Header("Настройка вращения")]
    public float targetAngle = 90f;
    public float dropSpeed = 5f;
    public Vector3 rotationAxis = Vector3.right;

    private bool isHolding = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private Coroutine dropCoroutine;

    void Start()
    {
        // Если забыли перетащить ссылку в инспекторе, скрипт возьмет тот объект, на котором висит
        if (ladderRoot == null) ladderRoot = transform;

        initialRotation = ladderRoot.localRotation;
        targetRotation = initialRotation * Quaternion.AngleAxis(targetAngle, rotationAxis);
        Debug.Log($"[Ladder] Старт. Объект вращения: {ladderRoot.name}");
    }

    public void OnGripStart(int mouseButton)
    {
        Debug.Log($"[Ladder] Крюк зацепился за {gameObject.name}!");
        isHolding = true;

        if (dropCoroutine == null)
        {
            dropCoroutine = StartCoroutine(DropRoutine());
        }
    }

    public void OnGripStop()
    {
        Debug.Log("[Ladder] Игрок отпустил кнопку мыши.");
        isHolding = false;
    }

    IEnumerator DropRoutine()
    {
        while (Quaternion.Angle(ladderRoot.localRotation, targetRotation) > 0.1f)
        {
            if (isHolding)
            {
                // Крутим именно ladderRoot, а не саму ручку!
                ladderRoot.localRotation = Quaternion.Slerp(ladderRoot.localRotation, targetRotation, Time.deltaTime * dropSpeed);
            }
            yield return null;
        }

        ladderRoot.localRotation = targetRotation;
        dropCoroutine = null;
        Debug.Log("[Ladder] Лестница полностью опущена!");
    }
}
