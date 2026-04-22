using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки луча")]
    public float interactDist = 4f; // Дистанция взаимодействия

    [Header("Настройки прицела")]
    public Image cursorDot; // Иконка точки в центре экрана
    public Color normalColor = Color.white;
    public Color interactColor = Color.red;

    [Header("Настройки GripClaws (Правая рука)")]
    public GameObject rightArmShoulder; // Сюда тянем плечо/предплечье (arm_right)
    public GameObject rightHandModel;   // Сюда тянем саму кисть (right_hand), если она не включилась

    void Start()
    {
        // Выключаем правую руку при старте игры
        if (rightArmShoulder != null) rightArmShoulder.SetActive(false);
        if (rightHandModel != null) rightHandModel.SetActive(false);

        StartCoroutine(InteractionRoutine());
    }

    IEnumerator InteractionRoutine()
    {
        while (true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactDist);

            // Сбрасываем переменные перед каждой проверкой
            LockButton button = null;
            DoorControl door = null;
            bool isPickableKey = false;
            bool isRightHandItem = false;

            if (hitSomething)
            {
                // Проверяем компоненты
                button = hit.collider.GetComponent<LockButton>();
                door = hit.collider.GetComponent<DoorControl>();

                // Проверка ключей по тегам
                if (hit.collider.CompareTag("KeyStaffOnly") || hit.collider.CompareTag("KeyForMiniMarket"))
                {
                    isPickableKey = true;
                }

                // ПРОВЕРКА: Навелись ли мы на предмет "Рука"
                if (hit.collider.CompareTag("Item_Right_hand"))
                {
                    isRightHandItem = true;
                }
            }

            // ЛОГИКА ЦВЕТА ПРИЦЕЛА
            if (button != null || door != null || isPickableKey || isRightHandItem)
            {
                if (cursorDot != null) cursorDot.color = interactColor;

                // ЛОГИКА НАЖАТИЯ КНОПКИ "E"
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Inventory inventory = GetComponent<Inventory>();

                    if (isRightHandItem)
                    {
                        // 1. Активируем части руки у игрока
                        if (rightArmShoulder != null) rightArmShoulder.SetActive(true);
                        if (rightHandModel != null) rightHandModel.SetActive(true);

                        // 2. Удаляем предмет с пола
                        Destroy(hit.collider.gameObject);

                        Debug.Log("Правая рука подобрана и активирована!");
                    }
                    else if (isPickableKey)
                    {
                        if (inventory != null) inventory.AddKey(hit.collider.gameObject);
                    }
                    else if (door != null)
                    {
                        if (inventory != null) door.TryOpen(inventory);
                    }
                    else if (button != null)
                    {
                        button.PressButton();
                    }
                }
            }
            else
            {
                if (cursorDot != null) cursorDot.color = normalColor;
            }

            yield return null;
        }
    }
}
