using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Для работы с TextMesh Pro в Unity 6

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI tipText; // Новое поле для текста подсказки

    [Header("Список подсказок")]
    [TextArea(2, 4)] // Делает поля в инспекторе удобными для чтения текста
    [SerializeField]
    private string[] loadingTips = new string[]
    {
        "Жалко что на эту игру потратили один бутерброд",
        "Не трогайте бракованные детали руками.",
        "Директор Giggles ToyWorks следит за вашей эффективностью."
    };

    [Header("Настройки времени")]
    [SerializeField] private float minLoadingTime = 2.5f; // Немного увеличил, чтобы игрок успел прочитать текст
    [SerializeField] private float fillSpeed = 2.5f;

    void Start()
    {
        if (progressBar != null) progressBar.value = 0f;
        if (percentageText != null) percentageText.text = "0%";

        // Выбираем и показываем случайную подсказку из нашего списка
        ShowRandomTip();

        StartCoroutine(LoadTargetSceneAsync());
    }

    private void ShowRandomTip()
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            // Берем случайный индекс из массива
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        string sceneToLoad = SceneLoader.TargetSceneName;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(0);
            yield break;
        }

        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float currentProgress = 0f;

        // Цикл работает, пока сцена загружается В ПАМЯТЬ (до 0.9f)
        while (operation.progress < 0.9f || currentProgress < 0.99f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Плавно двигаем ползунок
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, fillSpeed * Time.deltaTime);

            if (progressBar != null) progressBar.value = currentProgress;
            if (percentageText != null) percentageText.text = Mathf.FloorToInt(currentProgress * 100f) + "%";

            yield return null;
        }

        // Дозаполняем до 100%
        if (progressBar != null) progressBar.value = 1f;
        if (percentageText != null) percentageText.text = "100%";

        // Ждем, пока дотикает минимальное время удержания экрана загрузки
        while (Time.time - startTime < minLoadingTime)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f); // Маленькая пауза для красоты

        // Финальная активация уровня
        operation.allowSceneActivation = true;
    }
}
