using UnityEngine;
using System.Collections; // Обязательно для корутин

public class Dla_Dveri : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(-2f, 0, 0); // Куда едем
    public float speed = 2f; // Скорость

    private Vector3 _targetPosition;
    private bool _isOpening = false; // Флаг, чтобы не запускать корутину дважды

    void Start()
    {
        // Считаем финальную точку один раз при старте
        _targetPosition = transform.localPosition + openOffset;
    }

    // Метод, который вызывается из CodeLock
    public void OpenDoor()
    {
        // Если дверь уже в процессе открытия, ничего не делаем
        if (!_isOpening)
        {
            StartCoroutine(MoveDoorRoutine());
        }
    }

    IEnumerator MoveDoorRoutine()
    {
        _isOpening = true;

        // Пока мы не достигли цели (с очень маленькой погрешностью)
        while (Vector3.Distance(transform.localPosition, _targetPosition) > 0.01f)
        {
            // Двигаем дверь
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                _targetPosition,
                speed * Time.deltaTime
            );

            // Ждем следующего кадра
            yield return null;
        }

        // Принудительно ставим в финальную точку в конце
        transform.localPosition = _targetPosition;
        _isOpening = false;

        Debug.Log("Дверь полностью открыта, корутина завершена.");
    }
}
