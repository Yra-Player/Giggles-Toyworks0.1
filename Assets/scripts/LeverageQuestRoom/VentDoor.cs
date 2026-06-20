using UnityEngine;
using System.Collections;

public class VentDoor : MonoBehaviour
{
    [Header("Ссылки на рычаги")]
    public LeftLever lever1;
    public LeftLever lever2;

    [Header("Настройки движения вверх")]
    public float openHeight = 3.5f;
    public float openSpeed = 2f;

    private Vector3 closedPosition;
    private Vector3 targetPosition;
    private bool isOpening = false;

    void OnEnable()
    {
        // Подписываемся на события активации рычагов
        if (lever1 != null) lever1.OnLeverActivated += CheckLevers;
        if (lever2 != null) lever2.OnLeverActivated += CheckLevers;
    }

    void OnDisable()
    {
        if (lever1 != null) lever1.OnLeverActivated -= CheckLevers;
        if (lever2 != null) lever2.OnLeverActivated -= CheckLevers;
    }

    void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition + Vector3.up * openHeight;

        // Дополнительная проверка на случай, если ссылки забыли перетащить в инспекторе
        if (lever1 == null || lever2 == null)
        {
            Debug.LogError($"<color=red>[ДВЕРЬ] Ошибка! Забыли перетащить Рычаги в инспектор объекта {gameObject.name}!</color>");
        }
    }

    // Метод вызывается каждый раз, когда ЛЮБОЙ из рычагов опускается до упора
    private void CheckLevers()
    {
        if (lever1 != null && lever2 != null)
        {
            // Если оба рычага СЕЙЧАС находятся в активированном состоянии
            if (lever1.IsActivated() && lever2.IsActivated())
            {
                if (!isOpening)
                {
                    StartCoroutine(OpenDoorRoutine());
                }
            }
            else
            {
                Debug.Log($"<color=yellow>[ДВЕРЬ] Один из рычагов активирован, ждём второй... (Рычаг1: {lever1.IsActivated()}, Рычаг2: {lever2.IsActivated()})</color>");
            }
        }
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpening = true;
        Debug.Log("<color=cyan>[ДВЕРЬ] Условие выполнено! Открываем вентиляцию...</color>");

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log("<color=green>[ДВЕРЬ] Вентиляция успешно открыта!</color>");
    }
}
