using System.Collections;
using UnityEngine;

public class FallHandler : MonoBehaviour
{
    [Header("Настройки высоты")]
    [SerializeField] private float concussionHeight = 6f;
    [SerializeField] private float lethalHeight = 13f;

    [Header("Эффекты контузии")]
    [SerializeField] private float shockSensitivityMultiplier = 0.2f;
    [SerializeField] private float shockSpeedMultiplier = 0.4f;

    [SerializeField] private AudioClip concussionRingingSound; // Звон в ушах

    // НОВАЯ НАСТРОЙКА: Приватный звук обычного жесткого приземления (без смерти)
    [SerializeField] private AudioClip regularLandSound;

    private FirstPersonMovement movement;
    private FirstPersonLook look;
    private Rigidbody rb;
    private AudioSource internalAudioSource;
    private bool isInAir = false;
    private float highestY;

    void Start()
    {
        movement = GetComponent<FirstPersonMovement>();
        look = GetComponentInChildren<FirstPersonLook>();
        rb = GetComponent<Rigidbody>();

        internalAudioSource = gameObject.AddComponent<AudioSource>();
        internalAudioSource.playOnAwake = false;
    }

    void Update()
    {
        bool properlyGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        if (!properlyGrounded && rb.linearVelocity.y < -0.1f)
        {
            if (!isInAir)
            {
                highestY = transform.position.y;
                isInAir = true;
            }
            if (transform.position.y > highestY) highestY = transform.position.y;
        }
        else if (properlyGrounded && isInAir)
        {
            float fallDistance = highestY - transform.position.y;
            EvaluateFall(fallDistance);
            isInAir = false;
        }
    }

    private void EvaluateFall(float distance)
    {
        // Если высота смертельная — звук обычного приземления НЕ ИГРАЕТ, включается только экран смерти
        if (distance >= lethalHeight)
        {
            if (DeathScreenManager.Instance != null) DeathScreenManager.Instance.TriggerDeath();
        }
        // Если упал прилично, но выжил — играет и обычный глухой удар ног, и звон контузии
        else if (distance >= concussionHeight)
        {
            if (internalAudioSource != null && regularLandSound != null)
            {
                internalAudioSource.PlayOneShot(regularLandSound);
            }
            StartCoroutine(ApplyConcussion());
        }
    }

    private IEnumerator ApplyConcussion()
    {
        if (internalAudioSource != null && concussionRingingSound != null)
        {
            internalAudioSource.clip = concussionRingingSound;
            internalAudioSource.loop = false;
            internalAudioSource.Play();
        }

        float originalSensitivity = look.sensitivity;
        System.Func<float> concussionSpeedOverride = () => movement.speed * shockSpeedMultiplier;
        movement.speedOverrides.Add(concussionSpeedOverride);

        look.sensitivity = originalSensitivity * shockSensitivityMultiplier;

        float duration = 6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            look.sensitivity = Mathf.Lerp(originalSensitivity * shockSensitivityMultiplier, originalSensitivity, elapsed / duration);
            yield return null;
        }

        look.sensitivity = originalSensitivity;
        movement.speedOverrides.Remove(concussionSpeedOverride);
    }
}
