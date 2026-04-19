using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // Для работы с Image и Text
using System.Collections;

public class CodeLock : MonoBehaviour
{
    [Header("Настройки кода")]
    public string correctCode = "1111";
    public UnityEvent onCodeCorrect;

    [Header("Интерфейс экрана")]
    public Text displayTextField;   // Ссылка на текст
    public Image displayBackground; // Ссылка на фон (Image)

    [Header("Цвета")]
    public Color defaultColor = Color.gray;
    public Color winColor = Color.green;
    public Color failColor = Color.red;

    private string _currentInput = "";
    private bool _isUnlocked = false;

    void Start()
    {
        UpdateDisplay("ENTER"); // Начальная надпись
        if (displayBackground != null) displayBackground.color = defaultColor;
    }

    public void AddDigit(string digit)
    {
        if (_isUnlocked) return;

        // Если это первая цифра после ошибки, сбрасываем цвет фона
        if (_currentInput == "") displayBackground.color = defaultColor;

        _currentInput += digit;
        UpdateDisplay(_currentInput);

        if (_currentInput.Length >= 4)
        {
            CheckCode();
        }
    }

    private void CheckCode()
    {
        if (_currentInput == correctCode)
        {
            _isUnlocked = true;
            displayBackground.color = winColor;
            UpdateDisplay("OPEN");
            onCodeCorrect?.Invoke();
        }
        else
        {
            StartCoroutine(ShowErrorRoutine());
        }
    }

    // Корутина, чтобы экран мигнул красным и очистился
    IEnumerator ShowErrorRoutine()
    {
        displayBackground.color = failColor;
        UpdateDisplay("ERR");

        yield return new WaitForSeconds(1f); // Ждем секунду

        if (!_isUnlocked) // Если за это время ничего не изменилось
        {
            _currentInput = "";
            UpdateDisplay("----");
            displayBackground.color = defaultColor;
        }
    }

    private void UpdateDisplay(string text)
    {
        if (displayTextField != null) displayTextField.text = text;
    }
}
