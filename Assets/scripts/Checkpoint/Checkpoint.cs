using UnityEngine;
using System;

public class Checkpoint : MonoBehaviour
{
    [Header("Настройка локации")]
    public string locationName = "Название локации";
    public int slotIndex = 1; //номер слота
    
    
    
    [Header("Точка спавна игрока")]
    [Tooltip("Создайте внутри чекпоинта пустой объект (безопасное место на полу) и перетащите сюда")]
    public Transform spawnPoint;

    private bool _isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_isTriggered && other.CompareTag("Player"))
        {
            if (spawnPoint != null)
            {
                string prefix = "Slot_" + slotIndex + "_";

                // 1. Сохраняем координаты
                PlayerPrefs.SetFloat("CheckpointX", spawnPoint.position.x);
                PlayerPrefs.SetFloat("CheckpointY", spawnPoint.position.y);
                PlayerPrefs.SetFloat("CheckpointZ", spawnPoint.position.z);
                
                // 2. Сохраняем методанные в стиле Поповича Плейтам
                PlayerPrefs.SetString(prefix + "LocationName", locationName);
                //Дата сейва
                string currentDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                PlayerPrefs.SetString(prefix + "SaveDate", currentDate);

                //Сохраняем время проведённое в игре из менеджера
                CheckpointManager manager = FindFirstObjectByType<CheckpointManager>();
                if (manager != null)
                {
                    PlayerPrefs.SetFloat(prefix + "Playtime", manager.GetCurrentPlayTime());
                }

                // 3. Теперь мы знаем что этот слот существует
                PlayerPrefs.SetInt(prefix + "Exists", 1);

                //ОБновляем маркер для кнопки продолжить
                PlayerPrefs.SetInt("HasAnySave", 1);
                //Запоминаем последний слот чтобы загрузка знала его по умолч
                PlayerPrefs.SetInt("LastSelectedSlot", slotIndex);

                PlayerPrefs.Save();


                _isTriggered = true;
                Debug.Log($"[Checkpoint] Координаты сохранены в точке: {spawnPoint.position}");
            }
            else
            {
                Debug.LogError($"[Checkpoint] Ошибка: На объекте {gameObject.name} не назначен Spawn Point!");
            }
        }
    }
}
