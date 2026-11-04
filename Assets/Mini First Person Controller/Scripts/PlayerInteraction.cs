using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки луча")]
    public float interactDist = 4f;

    [Header("Настройки прицела")]
    public Image cursorDot;
    public Color normalColor = Color.white;
    public Color interactColor = Color.red;

    [Header("Настройки GripClaws (Правая рука)")]
    public GameObject rightArmShoulder;
    public GameObject rightHandModel;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasRightHand", 0) == 1)
        {
            if (rightArmShoulder != null) rightArmShoulder.SetActive(true);
            if (rightHandModel != null) rightHandModel.SetActive(true);
            Debug.Log("[PlayerInteraction] Правая рука восстановлена из сохранения при старте.");
        }
        else
        {
            if (rightArmShoulder != null) rightArmShoulder.SetActive(false);
            if (rightHandModel != null) rightHandModel.SetActive(false);
        }

        StartCoroutine(InteractionRoutine());
    }

    IEnumerator InteractionRoutine()
    {
        while (true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactDist);

            LockButton button = null;
            ButtonSequence soundButton = null;
            DoorControl door = null;
            LeftLever lever = null;
            TapeRecorder recorder = null;
            RemoteButton remoteBtn = null; // ИНТЕГРАЦИЯ: переменная для пульта охраны
            bool isPickableKey = false;
            bool isRightHandItem = false;

            if (hitSomething)
            {
                button = hit.collider.GetComponent<LockButton>();
                soundButton = hit.collider.GetComponent<ButtonSequence>();
                door = hit.collider.GetComponent<DoorControl>();
                lever = hit.collider.GetComponent<LeftLever>();
                recorder = hit.collider.GetComponent<TapeRecorder>();
                remoteBtn = hit.collider.GetComponent<RemoteButton>(); // ИНТЕГРАЦИЯ: считываем пульт при наведении

                if (hit.collider.CompareTag("KeyStaffOnly") || hit.collider.CompareTag("KeyForMiniMarket"))
                {
                    isPickableKey = true;
                }

                if (hit.collider.CompareTag("Item_Right_hand"))
                {
                    isRightHandItem = true;
                }
            }

            // ИНТЕГРАЦИЯ: прицел теперь краснеет и при наведении на пульт (remoteBtn != null)
            if (button != null || soundButton != null || door != null || lever != null || recorder != null || remoteBtn != null || isPickableKey || isRightHandItem)
            {
                if (cursorDot != null) cursorDot.color = interactColor;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Inventory inventory = GetComponent<Inventory>();

                    if (isRightHandItem)
                    {
                        if (rightArmShoulder != null) rightArmShoulder.SetActive(true);
                        if (rightHandModel != null) rightHandModel.SetActive(true);

                        PlayerPrefs.SetInt("HasRightHand", 1);
                        PlayerPrefs.Save();

                        Destroy(hit.collider.gameObject);
                        Debug.Log("[PlayerInteraction] Правая рука подобрана, активирована и сохранена в сейв!");
                    }
                    else if (isPickableKey)
                    {
                        if (inventory != null) inventory.AddKey(hit.collider.gameObject);
                    }
                    else if (door != null)
                    {
                        if (inventory != null) door.TryOpen(inventory);
                    }
                    else if (button != null)
                    {
                        button.PressButton();
                    }
                    // ИНТЕГРАЦИЯ: логика клика по кнопке пульта
                    else if (remoteBtn != null)
                    {
                        remoteBtn.PressButton();
                    }
                    else if (soundButton != null)
                    {
                        soundButton.OnButtonPress();
                    }
                    else if (lever != null)
                    {
                        lever.PullLever();
                    }
                    else if (recorder != null)
                    {
                        recorder.InsertTape();
                    }
                }
            }
            else
            {
                if (cursorDot != null) cursorDot.color = normalColor;
            }

            yield return null;
        }
    }
}
