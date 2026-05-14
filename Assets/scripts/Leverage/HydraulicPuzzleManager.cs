using System.Collections;
using UnityEngine;

public class HydraulicPuzzleManager : MonoBehaviour
{
    [Header("Ссылки на рычаги")]
    public LeftLever leftLever;
    public LeftLever rightLever;

    [Header("Настройки перегрузки системы")]
    public float overloadDuration = 1.5f;
    public float shakeIntensity = 0.1f;

    [Header("Элементы решетки (Физика)")]
    public Rigidbody ventGridRigidbody;
    public Vector3 ejectForce = new Vector3(-5f, 2f, 0f);

    [Header("Звуковые эффекты")]
    public AudioSource overloadAudio;
    public AudioSource ambientVentAudio;

    private bool isPuzzleSolved = false;
    private Vector3 cameraOrgPos;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null) cameraOrgPos = mainCamera.transform.localPosition;
        if (ventGridRigidbody != null) ventGridRigidbody.isKinematic = true;
        if (ambientVentAudio != null) ambientVentAudio.enabled = false;

        if (leftLever != null) leftLever.OnLeverActivated += CheckPuzzleCondition;
        if (rightLever != null) rightLever.OnLeverActivated += CheckPuzzleCondition;
    }

    private void CheckPuzzleCondition(LeftLever activatedLever)
    {
        if (isPuzzleSolved) return;

        if (leftLever != null && rightLever != null && leftLever.IsActivated() && rightLever.IsActivated())
        {
            StartCoroutine(OverloadSequence());
        }
    }
    public void CheckPuzzleConditionManual()
    {
        if (isPuzzleSolved) return;

        if (leftLever != null && rightLever != null)
        {
            if (leftLever.IsActivated() && rightLever.IsActivated())
            {
                StartCoroutine(OverloadSequence());
            }
        }
    }

    IEnumerator OverloadSequence()
    {
        isPuzzleSolved = true;
        Debug.Log("<color=red>СИСТЕМА ПЕРЕГРУЖЕНА! Начинается вибрация...</color>");

        if (overloadAudio != null) overloadAudio.Play();

        float elapsed = 0f;
        while (elapsed < overloadDuration && mainCamera != null)
        {
            elapsed += Time.deltaTime;

            float randomX = Random.Range(-1f, 1f) * shakeIntensity;
            float randomY = Random.Range(-1f, 1f) * shakeIntensity;
            mainCamera.transform.localPosition = cameraOrgPos + new Vector3(randomX, randomY, 0f);
            yield return null;
        }

        if (mainCamera != null) mainCamera.transform.localPosition = cameraOrgPos;
        if (overloadAudio != null) overloadAudio.Stop();

        // Отцепление Граб-пака
        GripclawsManager manager = FindObjectOfType<GripclawsManager>();
        if (manager != null && manager.hands != null)
        {
            foreach (var hand in manager.hands)
            {
                if (hand != null) hand.isAttached = false;
            }
        }

        // Выбивание решетки вентиляции
        if (ventGridRigidbody != null)
        {
            ventGridRigidbody.isKinematic = false;
            ventGridRigidbody.AddForce(ejectForce, ForceMode.Impulse);
            ventGridRigidbody.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 10f), ForceMode.Impulse);
            Debug.Log("<color=orange>Решетка вентиляции отлетела со звоном!</color>");
        }

        if (ambientVentAudio != null)
        {
            ambientVentAudio.enabled = true;
            ambientVentAudio.Play();
        }
    }

    // Отписка от событий при уничтожении объекта, чтобы избежать утечек памяти
    void OnDestroy()
    {
        if (leftLever != null) leftLever.OnLeverActivated -= CheckPuzzleCondition;
        if (rightLever != null) rightLever.OnLeverActivated -= CheckPuzzleCondition;
    }
}
