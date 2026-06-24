using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    [Header("Настройки вращения")]
    [Tooltip("Скорость, с которой крутится стенд с котом")]
    public float rotationSpeed = 15f;

    [Header("Состояние платформы")]
    [Tooltip("Если галочка стоит — платформа крутится. Триггером можно её отключить.")]
    public bool isRotating = true;

    void Update()
    {
        // Если вращение разрешено, крутим объект вокруг оси Y
        if (isRotating)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    // Этот метод можно будет вызвать из скрипта-триггера, чтобы остановить стенд
    public void StopPlatform()
    {
        isRotating = false;
    }
}
