using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    public bool isGamePaused = false;

    private PauseManager pauseManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        FindPauseManager();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (pauseManager == null)
        {
            FindPauseManager();
            if (pauseManager == null) return;
        }

        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            pauseManager.PauseGame();
        }
        else
        {
            pauseManager.ResumeGame();
        }
    }

    private void FindPauseManager()
    {
        pauseManager = Object.FindFirstObjectByType<PauseManager>();
    }
}
