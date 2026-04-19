using UnityEngine;

public class LockButton : MonoBehaviour
{
    public CodeLock panel;   // Ссылка на основной скрипт панели
    public string digit;     // Какую цифру присылает эта кнопка (напр. "1")
    public float interactDist = 3f;

    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Если игрок рядом и нажал E, глядя на кнопку (или просто нажал E в триггере)
        if (Vector3.Distance(transform.position, _player.position) < interactDist)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                panel.AddDigit(digit);
            }
        }
    }
}
