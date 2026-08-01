using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // Сюда мы временно записываем имя сцены, в которую хотим попасть
    public static string TargetSceneName { get; private set; }

    public static void Load(string sceneName)
    {
        TargetSceneName = sceneName;

        // Переключаемся на сцену загрузки. 
        // Название в кавычках должно ТОЧНО совпадать с именем вашей сцены загрузки в Unity!
        SceneManager.LoadScene("LoadingScene");
    }
}
