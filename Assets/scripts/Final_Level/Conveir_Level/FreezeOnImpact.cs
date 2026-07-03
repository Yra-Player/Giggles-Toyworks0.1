using UnityEngine;

public class FreezeOnImpact : MonoBehaviour
{
    private Rigidbody rb;
    private bool isFrozen = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Автоматически вызывается Unity в момент физического удара коллайдеров
    void OnCollisionEnter(Collision collision)
    {
        // Если лестница уже зафиксирована или люк еще не открылся (она кинематическая) — игнорируем
        if (isFrozen || rb == null || rb.isKinematic) return;

        // Игнорируем столкновения с руками игрока или самим игроком во время полета
        if (collision.collider.CompareTag("Player") || collision.collider.name.Contains("Hand")) return;

        // Фиксируем лестницу
        isFrozen = true;
        rb.isKinematic = true; // Полностью отключаем просчет физики
        rb.linearVelocity = Vector3.zero; // Гасим остаточную скорость
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"[Ladder] Лестница успешно приземлилась на объект {collision.gameObject.name} и заморожена!");
    }
}
