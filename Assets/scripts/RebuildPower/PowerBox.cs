using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBox : MonoBehaviour, IGripInteractable
{
    public ScannerPower targetScan;
    public ParticleSystem sparksEffect; //это на будущее для эффектов

    private Coroutine _powerRoutine;

    public void StartProvidingPower()
    {
        if (_powerRoutine == null)
        {
            _powerRoutine = StartCoroutine(PowerTickRoutine());
        }
    }

    public void StopProvidingPower()
    {
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
        float timeToFix = 2.0f;

        // Визуальный тест: меняем цвет щитка на желтый, пока чиним
        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer != null ? renderer.material.color : Color.white;
        if (renderer != null) renderer.material.color = Color.yellow;

        if (sparksEffect != null) sparksEffect.Play();

        while (chargeTimer < timeToFix)
        {
            chargeTimer += Time.deltaTime;

            // ВЫВОД В КОНСОЛЬ: Ты увидишь проценты внизу экрана Unity
            Debug.Log($"<color=yellow>ЗАРЯДКА ЩИТКА: {Mathf.Round((chargeTimer / timeToFix) * 100)}%</color>");

            yield return null;
        }

        // КОГДА ЗАКОНЧИЛИ:
        if (targetScan != null)
        {
            targetScan.Restore();
            targetScan.SetPermanentPower();
        }

        if (renderer != null) renderer.material.color = Color.green; // Стал зеленым - значит починен
        if (sparksEffect != null) sparksEffect.Stop();

        Debug.Log("<color=green>⚡ ПИТАНИЕ ВОССТАНОВЛЕНО НАВСЕГДА!</color>");
        _powerRoutine = null;
    }

    public void OnGripStart() => StartProvidingPower();
    public void OnGripStop() => StopProvidingPower();
}
