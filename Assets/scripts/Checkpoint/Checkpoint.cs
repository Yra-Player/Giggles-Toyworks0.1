using UnityEngine;
using System;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройка локации")]
    public string locationName = "Название локации";
    [Tooltip("Порядковый номер в цепочке (1, 2, 3...). Чекпоинт №2 не включится, пока не наступили на №1!")]
    public int slotIndex = 1;

    [Header("Точка спавна игрока")]
    [Tooltip("Создайте внутри чекпоинта пустой объект (безопасное место на полу) и перетащите сюда")]
    public Transform spawnPoint;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggered) return;

        string prefix = "Slot_" + slotIndex + "_";

        if (PlayerPrefs.GetInt(prefix + "Exists", 0) == 1)
        {
            _isTriggered = true;//блокировка тригера для сохранения ресорсов на проверки
            return;
        }

        if (other.CompareTag("Player"))
        {
            // --- ПРОДВИНУТАЯ ПРОВЕРКА ПОСЛЕДОВАТЕЛЬНОСТИ ---
            // Если это НЕ самый первый чекпоинт игры, проверяем его предшественника
            if (slotIndex > 1)
            {
                string previousPrefix = "Slot_" + (slotIndex - 1) + "_";

                // Смотрим, существует ли сохранение для предыдущего индекса
                bool isPreviousActivated = PlayerPrefs.GetInt(previousPrefix + "Exists", 0) == 1;

                if (!isPreviousActivated)
                {
                    // Если предыдущий чекпоинт еще не нажат, игнорируем игрока
                    Debug.Log($"<color=orange>[Checkpoint {slotIndex}] Заблокирован. Сначала нужно активировать Чекпоинт №{slotIndex - 1}!</color>");
                    return;
                }
            }
            // ------------------------------------------------

            if (spawnPoint != null)
            {

                // --- ЗАЩИТА ОТ ПЕРЕЗАПИСИ ИЗ БУДУЩЕГО ---
                int sourceSlot = PlayerPrefs.GetInt("SelectedSlotToLoad", 1);
                float previousPlayTime = 0f;

                if (sourceSlot > 0)
                {
                    previousPlayTime = PlayerPrefs.GetFloat("Slot_" + sourceSlot + "_PlayTime", 0f);
                }

                float currentSessionTime = Time.timeSinceLevelLoad;
                float totalPlayTime = previousPlayTime + currentSessionTime;

                if (PlayerPrefs.GetInt(prefix + "Exists", 0) == 1)
                {
                    float existingPlayTime = PlayerPrefs.GetFloat(prefix + "PlayTime", 0f);
                    if (existingPlayTime > totalPlayTime)
                    {
                        _isTriggered = true;
                        Debug.Log($"<color=yellow>[Checkpoint {slotIndex}] Игнорирование. В слоте уже есть более позднее сохранение.</color>");
                        return;
                    }
                }
                // ----------------------------------------

                // 1. Сохраняем координаты для слота и общие
                PlayerPrefs.SetFloat(prefix + "CheckpointX", spawnPoint.position.x);
                PlayerPrefs.SetFloat(prefix + "CheckpointY", spawnPoint.position.y);
                PlayerPrefs.SetFloat(prefix + "CheckpointZ", spawnPoint.position.z);

                PlayerPrefs.SetFloat("CheckpointX", spawnPoint.position.x);
                PlayerPrefs.SetFloat("CheckpointY", spawnPoint.position.y);
                PlayerPrefs.SetFloat("CheckpointZ", spawnPoint.position.z);

                // 2. Метаданные и время
                PlayerPrefs.SetString(prefix + "LocationName", locationName);
                string currentDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                PlayerPrefs.SetString(prefix + "SaveDate", currentDate);
                PlayerPrefs.SetFloat(prefix + "PlayTime", totalPlayTime);

                // 3. Маркеры
                PlayerPrefs.SetInt(prefix + "Exists", 1);
                PlayerPrefs.SetInt("HasAnySave", 1);
                int historicalMaxSlot = PlayerPrefs.GetInt("LastSelectedSlot", 0);
                if (slotIndex > historicalMaxSlot)
                {
                    PlayerPrefs.SetInt("LastSelectedSlot", slotIndex);
                }

                PlayerPrefs.Save();

                _isTriggered = true;
                Debug.Log($"<color=green>[Checkpoint {slotIndex}] Успешно активирован в цепочке! Время: {totalPlayTime} сек.</color>");
            }
            else
            {
                Debug.LogError($"[Checkpoint] Ошибка: На объекте {gameObject.name} не назначен Spawn Point!");
            }
        }
    }
}
