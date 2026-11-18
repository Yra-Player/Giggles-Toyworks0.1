using UnityEngine;

public class GripclawAudio : MonoBehaviour
{
    [Header("Настройки скорости и тона (Pitch)")]
    [Tooltip("Начальная скорость звука вылета лебедки")]
    public float flyStartPitch = 1.35f;

    [Tooltip("Максимальная скорость звука вылета при полном натяжении")]
    public float flyMaxPitch = 1.65f;

    [Tooltip("Скорость звука сматывания троса обратно")]
    public float returnPitch = 1.55f;

    [Tooltip("Тональность звука отлипания (меньше 1 = грубее и ниже)")]
    [Range(0.4f, 1.2f)]
    public float detachPitch = 0.72f;

    private GripclawsManager manager;

    void Awake()
    {
        manager = GetComponent<GripclawsManager>();
    }

    // 1. ЗВУК ВЫЛЕТА (Ускоренная лебедка + динамическое натяжение)
    public void PlayFlyAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null || hand.flySound == null) return;

        hand.audioSource.clip = hand.flySound;
        hand.audioSource.loop = true;
        hand.audioSource.pitch = flyStartPitch;
        hand.audioSource.Play();
    }

    public void UpdateFlyTensionAudio(GripclawHand hand)
    {
        if (hand == null || !hand.isFlying || hand.audioSource == null || hand.flySound == null) return;
        if (hand.audioSource.clip != hand.flySound) return;

        float currentDist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
        float tensionRatio = Mathf.Clamp01(currentDist / manager.maxDistance);

        hand.audioSource.pitch = Mathf.Lerp(flyStartPitch, flyMaxPitch, tensionRatio);
    }

    // 2. ЗВУК ЗАЦЕПА (Удар ладони о предмет)
    public void PlayAttachAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null) return;
        hand.audioSource.Stop();
        hand.audioSource.loop = false;
        hand.audioSource.pitch = 1f;

        if (hand.attachSound != null)
        {
            hand.audioSource.PlayOneShot(hand.attachSound);
        }
    }

    // 3. ЗВУК СМОТКИ (Ускоренный возврат ладони домой)
    public void PlayReturnAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null || hand.returnSound == null) return;

        hand.audioSource.clip = hand.returnSound;
        hand.audioSource.loop = true;
        hand.audioSource.pitch = returnPitch;
        hand.audioSource.Play();
    }

    // 4. ЗВУК ОТЦЕПЛЕНИЯ (Пониженный грубый щелчок)
    public void PlayDetachAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null) return;

        if (hand.detachSound != null)
        {
            hand.audioSource.pitch = detachPitch;
            hand.audioSource.PlayOneShot(hand.detachSound);
        }
    }

    // 5. ОСТАНОВКА ВСЕХ ЗВУКОВ РУКИ
    public void StopAudio(GripclawHand hand)
    {
        if (hand != null && hand.audioSource != null)
        {
            hand.audioSource.Stop();
            hand.audioSource.pitch = 1f;
        }
    }
}
