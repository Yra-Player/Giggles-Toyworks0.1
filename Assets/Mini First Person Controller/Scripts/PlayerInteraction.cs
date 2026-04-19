using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки луча")]
    public float interactDist = 4f; // Дистанция, на которой игрок видит предметы

    [Header("Настройки прицела")]
    public Image cursorDot; // Твой UI Image (точка в центре экрана)
    public Color normalColor = Color.white; // Цвет прицела в покое
    public Color interactColor = Color.red;  // Цвет прицела при наведении на предмет

    void Start()
    {
        // Запускаем бесконечный цикл проверки взаимодействия
        StartCoroutine(InteractionRoutine());
    }

    IEnumerator InteractionRoutine()
    {
        while (true)
        {
            // Создаем луч из центра камеры вперед
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Пускаем луч
            bool hitSomething = Physics.Raycast(ray, out hit, interactDist);

            // Временные переменные для найденных объектов
            LockButton button = null;
            DoorControl door = null;
            bool isPickableKey = false;

            if (hitSomething)
            {
                // 1. Проверяем, есть ли на объекте скрипты кнопки или двери
                button = hit.collider.GetComponent<LockButton>();
                door = hit.collider.GetComponent<DoorControl>();

                // 2. Проверяем, является ли объект одним из двух типов ключей по Тэгу
                if (hit.collider.CompareTag("KeyStaffOnly") || hit.collider.CompareTag("KeyForMiniMarket"))
                {
                    isPickableKey = true;
                }
            }

            // ЛОГИКА ВИЗУАЛА (Прицел)
            // Если перед нами кнопка, дверь или ключ — подсвечиваем прицел
            if (button != null || door != null || isPickableKey)
            {
                if (cursorDot != null) cursorDot.color = interactColor;

                // ЛОГИКА ВВОДА (Нажатие E)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Inventory inventory = GetComponent<Inventory>();

                    if (isPickableKey)
                    {
                        // Если это ключ — добавляем в инвентарь (скрипт инвентаря сам его удалит)
                        if (inventory != null) inventory.AddKey(hit.collider.gameObject);
                    }
                    else if (door != null)
                    {
                        // Если это дверь — пытаемся открыть (передаем инвентарь для проверки ключа)
                        if (inventory != null) door.TryOpen(inventory);
                    }
                    else if (button != null)
                    {
                        // Если кнопка — просто жмем
                        button.PressButton();
                    }
                }
            }
            else
            {
                // Если смотрим в пустоту или на обычный объект — прицел обычный
                if (cursorDot != null) cursorDot.color = normalColor;
            }

            // Ждем один кадр перед следующим циклом
            yield return null;
        }
    }
}
