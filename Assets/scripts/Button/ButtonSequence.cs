using System.Collections;
using UnityEngine;

public class ButtonSequence : MonoBehaviour
{
    [Header("Источники звука")]
    public AudioSource loopingSound; 
    public AudioSource sirenSound;   
    public AudioSource hitSound;     

    [Header("Интерфейс финала")]
    public GameObject blackScreen;    

    [Header("Телепортация")]
    public GameObject playerObject;   
    public Transform teleportTarget;  

    [Header("Настройки")]
    public float delayBeforeHit = 2.5f; 

    private bool isPressed = false; 

    public void OnButtonPress()
    {
        if (isPressed) return; 
        isPressed = true;

        if (loopingSound != null) loopingSound.Stop();

        StartCoroutine(AudioSequence());
    }

    private IEnumerator AudioSequence()
    {
        if (sirenSound != null) sirenSound.Play();

        yield return new WaitForSeconds(delayBeforeHit);

        // 1. Включаем удар и черный экран
        if (hitSound != null) hitSound.Play();

        if (blackScreen != null)
        {
            blackScreen.SetActive(true);
        }

        // 2. МГНОВЕННО ВЫКЛЮЧАЕМ СИРЕНУ ЗДЕСЬ (как только экран потемнел)
        if (sirenSound != null)
        {
            sirenSound.Stop();
        }

        // 3. Ждем 2 секунды в темноте, пока доигрывает глухой удар
        yield return new WaitForSeconds(2.0f);

        // 4. ТЕЛЕПОРТАЦИЯ
        if (playerObject != null && teleportTarget != null)
        {
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerObject.transform.position = teleportTarget.position;
            playerObject.transform.rotation = teleportTarget.rotation;

            if (cc != null) cc.enabled = true;
        }

        yield return new WaitForSeconds(1.0f);

        if (blackScreen != null)
        {
            blackScreen.SetActive(false);
        }
    }
}
