using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки луча")]
    public float interactDist = 4f;

    [Header("Настройки прицела")]
    public Image cursorDot;
    public Color normalColor = Color.white;
    public Color interactColor = Color.red;

    void Start()
    {
        // Запускаем единственный поток управления
        StartCoroutine(InteractionRoutine());
    }

    IEnumerator InteractionRoutine()
    {
        while (true)
        {
            // 1. Пускаем луч
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactDist);
            LockButton button = null;

            if (hitSomething)
            {
                button = hit.collider.GetComponent<LockButton>();
            }

            // 2. Логика отображения прицела
            if (button != null)
            {
                if (cursorDot != null) cursorDot.color = interactColor;

                // 3. Проверка ввода внутри корутины
                if (Input.GetKeyDown(KeyCode.E))
                {
                    button.PressButton();
                }
            }
            else
            {
                if (cursorDot != null) cursorDot.color = normalColor;
            }

            // Ждем до следующего кадра (аналог Update, но внутри корутины)
            yield return null;
        }
    }
}
