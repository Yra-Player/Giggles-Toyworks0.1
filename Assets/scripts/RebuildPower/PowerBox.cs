using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PowerBox : MonoBehaviour, IGripInteractable
{
    [Header("Настройки щитка")]
    public Scanner targetScan;
    public float timeToFix = 2.0f;
    public ParticleSystem sparksEffect;

    [Header("События при починке")]
    public UnityEvent onPowerFixedUnity; // Сюда можно привязать обратное включение света

    private Coroutine _powerRoutine;
    private bool _isFixed = false;

    public void OnGripStart(int mouseButton)
    {
        if (_isFixed) return;

        if (_powerRoutine == null)
        {
            _powerRoutine = StartCoroutine(PowerTickRoutine());
        }
    }

    public void OnGripStop()
    {
        if (_isFixed) return;

        if (_powerRoutine != null)
        {
            StopCoroutine(_powerRoutine);
            _powerRoutine = null;
        }
        if (sparksEffect != null) sparksEffect.Stop();
    }

    private IEnumerator PowerTickRoutine()
    {
        float chargeTimer = 0f;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.yellow;

        if (sparksEffect != null) sparksEffect.Play();

        while (chargeTimer < timeToFix)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        _isFixed = true;
        _powerRoutine = null;

        if (targetScan != null)
        {
            targetScan.RestorePower();
        }

        if (rend != null) rend.material.color = Color.green;
        if (sparksEffect != null) sparksEffect.Stop();

        onPowerFixedUnity?.Invoke();
    }
}
