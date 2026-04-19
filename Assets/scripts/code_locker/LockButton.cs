using UnityEngine;

public class LockButton : MonoBehaviour
{
    public CodeLock panel;
    public string digit;

    // Этот метод теперь будет вызываться из другого скрипта
    public void PressButton()
    {
        if (panel != null)
        {
            panel.AddDigit(digit);
            Debug.Log("Нажата кнопка: " + digit);
        }
    }
}
