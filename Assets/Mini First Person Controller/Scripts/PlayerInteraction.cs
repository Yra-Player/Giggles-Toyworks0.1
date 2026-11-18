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

    private Inventory _inventory;

    private void Start()
    {
        _inventory = GetComponent<Inventory>();

        bool hasRightHand = PlayerPrefs.GetInt("HasRightHand", 0) == 1;
        SetRightHandActive(hasRightHand);

        if (hasRightHand)
        {
            Debug.Log("[PlayerInteraction] Правая рука восстановлена из сохранения при старте.");
        }
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDist))
        {
            Collider col = hit.collider;

            // Проверяем все интерактивные компоненты и теги
            LockButton button = col.GetComponent<LockButton>();
            DoorControl door = col.GetComponent<DoorControl>();
            LeftLever lever = col.GetComponent<LeftLever>();
            TapeRecorder recorder = col.GetComponent<TapeRecorder>();
            RemoteButton remoteBtn = col.GetComponent<RemoteButton>();

            bool isPickableKey = col.CompareTag("KeyStaffOnly") || col.CompareTag("KeyForMiniMarket");
            bool isRightHandItem = col.CompareTag("Item_Right_hand");

            bool canInteract = button != null || door != null || lever != null ||
                               recorder != null || remoteBtn != null ||
                               isPickableKey || isRightHandItem;

            if (canInteract)
            {
                SetCursorColor(interactColor);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    HandleInteraction(col.gameObject, button, door, lever, recorder, remoteBtn, isPickableKey, isRightHandItem);
                }
                return;
            }
        }

        SetCursorColor(normalColor);
    }

    private void HandleInteraction(
        GameObject targetObj,
        LockButton button,
        DoorControl door,
        LeftLever lever,
        TapeRecorder recorder,
        RemoteButton remoteBtn,
        bool isPickableKey,
        bool isRightHandItem)
    {
        if (isRightHandItem)
        {
            SetRightHandActive(true);
            PlayerPrefs.SetInt("HasRightHand", 1);
            PlayerPrefs.Save();

            Destroy(targetObj);
            Debug.Log("[PlayerInteraction] Правая рука подобрана, активирована и сохранена в сейв!");
        }
        else if (isPickableKey && _inventory != null)
        {
            _inventory.AddKey(targetObj);
        }
        else if (door != null && _inventory != null)
        {
            door.TryOpen(_inventory);
        }
        else if (button != null)
        {
            button.PressButton();
        }
        else if (remoteBtn != null)
        {
            remoteBtn.PressButton();
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

    private void SetRightHandActive(bool isActive)
    {
        if (rightArmShoulder != null) rightArmShoulder.SetActive(isActive);
        if (rightHandModel != null) rightHandModel.SetActive(isActive);
    }

    private void SetCursorColor(Color color)
    {
        if (cursorDot != null && cursorDot.color != color)
        {
            cursorDot.color = color;
        }
    }
}
