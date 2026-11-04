using UnityEngine;

public class RemoteButton : MonoBehaviour
{
    [Header("Какую дверь открывает эта кнопка пульта?")]
    public DoorControl targetDoor;

    [Header("Анимация кнопки")]
    public float pressDepth = 0.05f; // На сколько кнопка уходит вниз

    private bool isPressed = false;

    public void PressButton()
    {
        if (isPressed || targetDoor == null) return;

        isPressed = true;
        Debug.Log($"[Пульт] Кнопка {gameObject.name} активировала дверь {targetDoor.name}");

        // Визуально утапливаем кнопку вниз
        transform.localPosition -= new Vector3(0, pressDepth, 0);

        // Дергаем наш новый интегрированный метод в двери
        targetDoor.InteractFromPanel();
    }
}
