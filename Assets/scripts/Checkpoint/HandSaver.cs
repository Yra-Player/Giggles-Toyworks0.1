using UnityEngine;

public class HandSaver : MonoBehaviour
{
    [Header("Ссылки на объекты рук в иерархии игрока")]
    public GameObject leftHand;  // Перетащите сюда левую руку (arm_Left)
    public GameObject rightHand; // Перетащите сюда правую руку (arm_Right)

    void Start()
    {
        // Проверяем: если игрок загрузился через кнопку "Продолжить" (есть ключ чекпоинта)
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            // На конвейерах и дальше по сюжету у игрока ДОЛЖНЫ БЫТЬ обе руки.
            // Поэтому принудительно включаем их при спавне на чекпоинте!
            if (leftHand != null) leftHand.SetActive(true);
            if (rightHand != null) rightHand.SetActive(true);

            Debug.Log("Контроллер рук: Игрок загрузился с чекпоинта, обе руки принудительно активированы!");
        }
        else
        {
            // Если это "Новая игра" (сохранений нет) — оставляем ваше стартовое состояние.
            // Например, правая рука выключена, а левая включена.
            if (leftHand != null) leftHand.SetActive(true);
            if (rightHand != null) rightHand.SetActive(false);

            Debug.Log("Контроллер рук: Начата новая игра. Правая рука скрыта до востребования.");
        }
    }

    // Этот метод вы сможете вызвать в будущем ОДИН раз, когда игрок физически подберет правую руку на локации
    public void AuthorizeRightHand()
    {
        if (rightHand != null) rightHand.SetActive(true);
        Debug.Log("Контроллер рук: Игрок подобрал правую руку на локации!");
    }
}
