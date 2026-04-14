using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f; // Угол открытия
    public float smooth = 2f;    // Скорость вращения

    private bool isOpen = false;
    private bool isPlayerNearby = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // Запоминаем начальный поворот как "закрыто"
        closedRotation = transform.rotation;
        // Вычисляем поворот для "открыто" (вращение по оси Y)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        // Если игрок рядом и нажал E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        // Плавно вращаем дверь к нужному состоянию
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smooth);
    }

    // Проверка входа игрока в зону двери
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    // Проверка выхода
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}