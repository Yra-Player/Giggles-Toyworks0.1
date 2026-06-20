using System.Collections;
using UnityEngine;

public class StartDoor : MonoBehaviour
{
    [Header("Настройки доступа")]
    public bool isLocked = true;

    [Header("Анимация подъема")]
    public float openHeight = 3.0f; // Высота, на которую поднимется дверь
    public float speed = 2.0f;     // Скорость движения
    public bool autoClose = true;  // Закрывать автоматически?
    public float closeDelay = 3.0f; // Задержка перед закрытием

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isOpened = false;
    private bool isMoving = false;  // Защита от спама кнопкой Е

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * openHeight;
    }

    // Этот метод вызывается из вашего скрипта игрока при нажатии на E
    public void Interact(Inventory playerInventory)
    {
        if (isOpened || isMoving) return;

        // Проверяем ключ персонала, если дверь заперта
        if (!isLocked || playerInventory.HasStaffKey())
        {
            if (isLocked) playerInventory.UseStaffKey(); // Расходуем ключ

            isOpened = true;
            StartCoroutine(MoveDoor(targetPosition));
        }
        else
        {
            Debug.Log("Нужен ключ персонала!");
        }
    }

    private IEnumerator MoveDoor(Vector3 destination)
    {
        isMoving = true;

        // Плавное движение к заданной точке
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;

        // Если дверь открылась и включено автозакрытие — ждем и закрываем
        if (destination == targetPosition && autoClose)
        {
            yield return new WaitForSeconds(closeDelay);
            StartCoroutine(MoveDoor(startPosition));
        }
        // Если дверь вернулась в начальное положение — сбрасываем флаг открытия
        else if (destination == startPosition)
        {
            isOpened = false;
        }
    }
}
