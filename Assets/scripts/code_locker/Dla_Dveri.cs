using UnityEngine;
using System.Collections;

public class Dla_Dveri : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(-2f, 0, 0); // Куда едем
    public float speed = 2f; // Скорость

    private Vector3 _startPosition; // Запоминаем базовую закрытую точку
    private Vector3 _targetPosition;
    private bool _isOpening = false;
    private bool _isClosedAtStart = true;

    void Awake()
    {
        // 💡 ВАЖНО: Запоминаем стартовую позицию в Awake до того, как сработают проверки в Start!
        _startPosition = transform.localPosition;
        _targetPosition = _startPosition + openOffset;
    }

    // Метод для МГНОВЕННОГО открытия при загрузке сохранения
    public void InstantOpen()
    {
        _isClosedAtStart = false; // Помечаем, что на старте плавно ехать не нужно
        transform.localPosition = _startPosition + openOffset;
        Debug.Log($"<color=cyan>[ДВЕРЬ] {gameObject.name} мгновенно открыта из сохранения.</color>");
    }

    public void OpenDoor()
    {
        if (!_isOpening && _isClosedAtStart)
        {
            StartCoroutine(MoveDoorRoutine());
        }
    }

    IEnumerator MoveDoorRoutine()
    {
        _isOpening = true;

        while (Vector3.Distance(transform.localPosition, _targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                _targetPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }

        transform.localPosition = _targetPosition;
        _isOpening = false;

        Debug.Log("Дверь полностью открыта, корутина завершена.");
    }
}
