using UnityEngine;
using System.Collections.Generic;

public class DevCheat : MonoBehaviour
{
    [Header("Клавиша Открытия меню")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Настройки Грэбпака из ItemPickup")]
    [Tooltip("Имя корневого объекта Греппака на игроке (BuilversionGripClaws)")]
    [SerializeField] private string rootObjectName = "BuilversionGripClaws";

    private bool _showMenu = false;
    private Checkpoint[] _allCheckpoints;
    private GameObject _player;

    private List<string> _myLogs = new List<string>();
    private Vector2 _scrollPosition;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player"); //Автоматика нахождения игрока по тегу
        _allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None); //Атоматика для чекпоинтов
        System.Array.Sort(_allCheckpoints, (a, b) => a.slotIndex.CompareTo(b.slotIndex)); //Сортировка по индексу слота

        // ПОДПИСКА НА ЛОГИ: Заставляем Unity делиться сообщениями
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        // Обязательно отписываемся при уничтожении объекта, чтобы не было утечек памяти
        Application.logMessageReceived -= HandleLog;
    }

    // Этот метод вызывается автоматически каждый раз, когда срабатывает любой Debug.Log
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Warning) return;

        // ЖЕСТКИЙ ФИЛЬТР: Если в тексте сообщения есть слова про тени или атлас (твой спам со знаком "!")
        if (logString.Contains("shadow maps") || logString.Contains("shadow atlas") || logString.Contains("resolution"))
            return; // Полностью игнорируем этот лог и не выводим на экран

        string prefix = "[LOG] ";
        if (type == LogType.Error || type == LogType.Exception) prefix = "[🔥 ERROR] ";
        if (logString.Contains("DEV ЧИТ")) prefix = "[⚡ CHEAT] ";
        if (logString.Contains("Checkpoint") || logString.Contains("Grebpack")) prefix = "[💾 SAVE] ";

        // Очищаем text от HTML-тегов цвета, чтобы OnGUI выводил его корректно
        string cleanLog = logString.Replace("<color=yellow>", "").Replace("</color>", "")
                                   .Replace("<color=green>", "").Replace("</color>", "")
                                   .Replace("<color=cyan>", "").Replace("</color>", "")
                                   .Replace("<color=red>", "").Replace("</color>", "");

        _myLogs.Add(prefix + cleanLog);

        if (_myLogs.Count > 30)
        {
            _myLogs.RemoveAt(0);
        }
    }

    void Update()
    {
        //По нажатию вкл/выкл меню
        if (Input.GetKeyDown(toggleKey))
        {
            _showMenu = !_showMenu;

            //Вкл/выкл курсор мыши
            Cursor.lockState = _showMenu ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _showMenu;
        }
    }

    //Отрисовка быстрого UI поверх игры
    void OnGUI()
    {
        if (!_showMenu) return;

        //СОздаём окошко
        GUI.backgroundColor = Color.green;
        GUI.color = Color.white;

        //Размер окна
        Rect windowRect = new Rect(20, 20, 650, 480);
        GUILayout.Window(99, windowRect, DrawDevWindow, "⚡ РЕЖИМ РАЗРАБОТЧИКА И КОНСОЛЬ ЛОГОВ ⚡");
    }

    void DrawDevWindow(int windowID)
    {
        // НАСТРОЙКА ПОВЫШЕНИЯ КАЧЕСТВА ТЕКСТА
        GUI.skin.label.fontSize = 14;       // Увеличиваем размер обычного текста
        GUI.skin.button.fontSize = 13;      // Увеличиваем размер текста на кнопках
        GUI.skin.label.wordWrap = true;     // Разрешаем тексту красиво переноситься на новую строку

        GUILayout.BeginHorizontal();

        // --- ЛЕВАЯ КОЛОНКА: КНОПКИ ТЕЛЕПОРТА ---
        GUILayout.BeginVertical(GUILayout.Width(280));

        GUI.skin.label.fontSize = 16;
        GUILayout.Label("F1: Закрыть меню");
        GUI.skin.label.fontSize = 14;

        GUILayout.Space(5);
        GUILayout.Label($"Найдено чекпоинтов: {_allCheckpoints.Length}");
        GUILayout.Space(10);

        foreach (Checkpoint cp in _allCheckpoints)
        {
            string status = PlayerPrefs.GetInt("Slot_" + cp.slotIndex + "_Exists", 0) == 1 ? "✓ ЗАПИСАН" : "✕ ПУСТОЙ";

            //Создаём кнопку для каждого чекпоинта
            if (GUILayout.Button($"[{cp.slotIndex}] {cp.locationName}\n({status})", GUILayout.Height(40)))
            {
                TeleportToCheckpoint(cp);
            }
        }

        GUILayout.FlexibleSpace();

        // --- БЛОК: ВЫДАЧА СНАРЯЖЕНИЯ ---
        GUILayout.Label("🛠️ СНАРЯЖЕНИЕ ИГРОКА:");

        if (GUILayout.Button("🟢 ВЫДАТЬ ГРЭБПАК (ЛЕВАЯ ВКЛ.)", GUILayout.Height(30)))
        {
            CheatGiveHand(true);
        }
        if (GUILayout.Button("🔵 ВЫДАТЬ ПРАВУЮ РУКУ", GUILayout.Height(30)))
        {
            CheatGiveHand(false);
        }
        GUILayout.Space(10);
        // --------------------------------

        if (GUILayout.Button("ОЧИСТИТЬ ВСЕ СЕЙВЫ", GUILayout.Height(35)))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Checkpoint] Все сохранения стерты прямо из игры!");
        }
        GUILayout.EndVertical();

        GUILayout.Space(20);

        // --- ПРАВАЯ КОЛОНКА: НАША ЧИСТАЯ КОНСОЛЬ ЛОГОВ ---
        GUILayout.BeginVertical();

        GUI.skin.label.fontSize = 16;
        GUILayout.Label("📋 МОНИТОР СОБЫТИЙ И СОХРАНЕНИЙ:");
        GUI.skin.label.fontSize = 14;

        GUILayout.Space(5);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);

        foreach (string log in _myLogs)
        {
            if (log.StartsWith("[🔥")) GUI.contentColor = Color.red;
            else if (log.StartsWith("[⚡")) GUI.contentColor = Color.yellow;
            else if (log.StartsWith("[💾")) GUI.contentColor = Color.cyan;
            else GUI.contentColor = Color.white;

            GUILayout.Label(log);
        }
        GUI.contentColor = Color.white;

        GUILayout.EndScrollView();

        if (GUILayout.Button("Очистить консоль логов", GUILayout.Height(30)))
        {
            _myLogs.Clear();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void TeleportToCheckpoint(Checkpoint cp)
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");

        if (_player != null && cp.spawnPoint != null)
        {
            //Отключаем на кадр CharcterController если он есть иначе юнити запретит телепорт движением
            CharacterController cc = _player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            //Перемещаем игрок на точку спавна
            _player.transform.position = cp.spawnPoint.position;
            _player.transform.rotation = cp.spawnPoint.rotation;

            if (cc != null) cc.enabled = true;
            Debug.Log($"[DEV ЧИТ] Телепорт на Чекпоинт №{cp.slotIndex} ({cp.locationName})");
        }
        else
        {
            Debug.LogError("[DEV ЧИТ] Не удалось телепортироваться! Проверьте тег Player у игрока или Spawn Point на чекпоинте.");
        }
    }

    // УМНАЯ ЛОГИКА ЧИТ-ВЫДАЧИ РУК ГРЭБПАКА
    // ЛОГИКА ЧИТ-ВЫДАЧИ РУК ГРЭБПАКА (ИСПРАВЛЕНА ДЛЯ ЛЕВОЙ РУКИ)
    private void CheatGiveHand(bool isLeftHandWithGrappack)
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");

        if (_player == null)
        {
            Debug.LogError("[DEV ЧИТ] Игрок не найден на сцене! Проверьте тег Player.");
            return;
        }

        // Ищем корень Грэбпака на персонаже
        GameObject rootObj = FindChildRecursive(_player.transform, rootObjectName);
        if (rootObj == null)
        {
            Debug.LogError($"[DEV ЧИТ] Ошибка: На игроке не найден корневой объект Грэбпака '{rootObjectName}'!");
            return;
        }

        // Активируем сам корень Грэбпака
        rootObj.SetActive(true);

        if (isLeftHandWithGrappack)
        {
            PlayerPrefs.SetInt("HasLeftHand", 1);

            // ИСПРАВЛЕНИЕ: Автоматически находим и включаем объекты левой руки внутри Грэбпака
            // Ищем любые дочерние объекты, содержащие в имени "left" или "hand" (без учета регистра)
            foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
            {
                string childName = child.name.ToLower();
                if (childName.Contains("left") || childName.Contains("_l_") || childName.Contains("arm"))
                {
                    child.gameObject.SetActive(true);
                }
            }

            Debug.Log("[Grebpack ЧИТ] Игроку выдан Грэбпак и принудительно включена Левая рука!");
        }
        else
        {
            // МАГИЯ ДЛЯ ПРАВОЙ РУКИ: Стучимся напрямую в скрипт игрока PlayerInteraction!
            PlayerInteraction interaction = _player.GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = _player.GetComponentInChildren<PlayerInteraction>();

            if (interaction != null)
            {
                // Зажигаем те самые объекты, которые ты привязал в инспекторе скрипта взаимодействия
                if (interaction.rightArmShoulder != null) interaction.rightArmShoulder.SetActive(true);
                if (interaction.rightHandModel != null) interaction.rightHandModel.SetActive(true);

                PlayerPrefs.SetInt("HasRightHand", 1);
                Debug.Log("[Grebpack ЧИТ] Игроку выдана Правая рука напрямую через PlayerInteraction!");
            }
            else
            {
                Debug.LogError("[DEV ЧИТ] На игроке не найден скрипт PlayerInteraction! Выдача правой руки невозможна.");
            }
        }
        PlayerPrefs.Save();
    }

    private GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }
}
