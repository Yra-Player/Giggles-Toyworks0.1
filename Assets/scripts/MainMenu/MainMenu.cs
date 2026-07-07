using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Обязательно добавляем для работы с кнопками

public class MainMenu : MonoBehaviour
{
    [Header("Настройки сцены")]
    public string gameSceneName = "FactoryScene"; // Впишите сюда ТОЧНОЕ название игровой сцены

    [Header("Ссылки на UI кнопки")]
    public Button continueButton; // Перетащите сюда вашу кнопку "Продолжить" из иерархии

    void Start()
    {
        // Проверяем, запускал ли игрок игру раньше и доходил ли до чекпоинта
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            // Если сохранение есть, кнопка "Продолжить" работает
            if (continueButton != null) continueButton.interactable = true;
        }
        else
        {
            // Если сохранений нет, кнопку "Продолжить" нельзя нажать (она станет серой)
            if (continueButton != null) continueButton.interactable = false;
        }
    }

    // Этот метод вешаем на кнопку "НОВАЯ ИГРА"
    public void NewGame()
    {
        // Стираем старые координаты и статус руки, чтобы начать чисто с лобби/начала главы
        PlayerPrefs.DeleteKey("CheckpointX");
        PlayerPrefs.DeleteKey("CheckpointY");
        PlayerPrefs.DeleteKey("CheckpointZ");
        PlayerPrefs.DeleteKey("HasRightHand");
        PlayerPrefs.Save();

        // Запускаем игру с самого начала
        SceneManager.LoadScene(gameSceneName);
    }

    // Этот метод вешаем на кнопку "ПРОДОЛЖИТЬ"
    public void ContinueGame()
    {
        // Просто запускаем игровую сцену. 
        // Когда она откроется, CheckpointManager включится, увидит координаты в памяти и сам перенесет игрока!
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Открыты настройки");
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
}
