using UnityEngine;

public class GripclawAudio : MonoBehaviour
{
    private GripclawsManager manager;

    void Awake()
    {
        manager = GetComponent<GripclawsManager>();
    }

    // 1. ЗВУК ВЫЛЕТА (С динамическим натяжением)
    public void PlayFlyAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null || hand.flySound == null) return;

        hand.audioSource.clip = hand.flySound;
        hand.audioSource.loop = true;
        hand.audioSource.pitch = 1f; // Сбрасываем тон в дефолт
        hand.audioSource.Play();
    }

    // Этот метод менеджер будет вызывать в LateUpdate, чтобы менять звук по мере натяжения троса
    public void UpdateFlyTensionAudio(GripclawHand hand)
    {
        if (hand == null || !hand.isFlying || hand.audioSource == null || hand.flySound == null) return;

        // Считаем, на сколько процентов рука отлетела от игрока (от 0 до 1)
        float currentDist = Vector3.Distance(hand.startPoint.position, hand.handTransform.position);
        float tensionRatio = Mathf.Clamp01(currentDist / manager.maxDistance);

        // Идея натяжения: чем дальше летит рука, тем выше становится тон звука (от 1.0 до 1.25)
        // Это создаст крутой аудио-эффект натягивающегося провода
        hand.audioSource.pitch = Mathf.Lerp(1f, 1.25f, tensionRatio);
    }

    // 2. ЗВУК ЗАЦЕПА (Удар ладони о предмет)
    public void PlayAttachAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null) return;
        hand.audioSource.Stop(); // Тушим звук полета
        hand.audioSource.loop = false;
        hand.audioSource.pitch = 1f;

        if (hand.attachSound != null)
        {
            hand.audioSource.PlayOneShot(hand.attachSound);
        }
    }

    // 3. ЗВУК СМОТКИ (Возврат ладони домой)
    public void PlayReturnAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null || hand.returnSound == null) return;

        hand.audioSource.clip = hand.returnSound;
        hand.audioSource.loop = true;
        hand.audioSource.pitch = 1.1f; // Звук возврата сделаем чуть быстрее дефолта
        hand.audioSource.Play();
    }

    // 4. ЗВУК ОТЦЕПЛЕНИЯ
    public void PlayDetachAudio(GripclawHand hand)
    {
        if (hand == null || hand.audioSource == null) return;
        // Проигрываем короткий щелчок отцепления, не ломая текущие звуки смотки, если они накладываются
        if (hand.detachSound != null)
        {
            hand.audioSource.PlayOneShot(hand.detachSound);
        }
    }

    // 5. ОСТАНОВКА ВСЕХ ЗВУКОВ РУКИ
    public void StopAudio(GripclawHand hand)
    {
        if (hand != null && hand.audioSource != null)
        {
            hand.audioSource.Stop();
        }
    }
}
