using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    private Transform character;

    public float sensitivity = 2;
    public float smoothing = 1.5f;

    private Vector2 velocity;
    private Vector2 frameVelocity;

    void Reset()
    {
        // Автоматически находим компонент движения в родительских объектах
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Фиксируем курсор мыши в центре игрового экрана
        Cursor.lockState = CursorLockMode.Locked;

        // Синхронизируем направление мыши с текущим поворотом тела в сцене
        InitRotation();
    }

    // Этот метод вызывается ОДИН раз при старте или менеджером чекпоинтов,
    // чтобы принудительно направить взгляд мыши туда, куда повернут персонаж
    public void InitRotation()
    {
        if (character != null)
        {
            // Берем текущий Y-поворот персонажа и заносим его как стартовый угол для мыши
            velocity.x = character.localEulerAngles.y;
            velocity.y = 0f; // Взгляд выравнивается строго по горизонту
            frameVelocity = Vector2.zero;
        }
    }

    // Метод Update обязателен: он каждую секунду считывает движения мыши игрока
    void Update()
    {
        // Получаем сдвиг мыши за текущий кадр
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);

        // Сглаживаем движение
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;

        // Ограничиваем взгляд вверх и вниз (чтобы голова не закручивалась на 360 градусов)
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Вращаем камеру вверх-вниз, а тело персонажа — влево-вправо
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
