using System.Collections;
using UnityEngine;

public class CeilingHatch : MonoBehaviour, IGripInteractable
{
    [Header("Ссылки")]
    [Tooltip("Rigidbody самого люка, который будет отрываться и падать.")]
    public Rigidbody hatchRigidbody;

    [Header("Тайминги")]
    public float holdDuration = 3f;

    [Header("Настройки импульса (Необязательно)")]
    [Tooltip("Сила, с которой люк выталкивается вниз или в сторону в момент отрыва.")]
    public Vector3 pushForce = new Vector3(0f, -2f, 0f);

    private bool isActivated = false;

    void Start()
    {
        // В начале игры гарантируем, что люк жестко зафиксирован на потолке
        if (hatchRigidbody != null)
        {
            hatchRigidbody.isKinematic = true;
        }
    }

    public void OnGripStart(int mouseButton)
    {
        if (!isActivated)
        {
            isActivated = true;
            StartCoroutine(HatchDetachRoutine());
        }
    }

    public void OnGripStop()
    {
        // Логика при отпускании кнопки (если требуется прерывать удержание, можно добавить сюда)
    }

    IEnumerator HatchDetachRoutine()
    {
        // Ждем заданное время удержания
        yield return new WaitForSeconds(holdDuration);

        if (hatchRigidbody != null)
        {
            // Освобождаем физику люка — теперь он подвержен гравитации и падает
            hatchRigidbody.isKinematic = false;

            // Добавляем начальный толчок, чтобы люк отвалился более эффектно
            hatchRigidbody.AddForce(pushForce, ForceMode.VelocityChange);
        }
    }
}
