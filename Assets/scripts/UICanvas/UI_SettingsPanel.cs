using UnityEngine;
using UnityEngine.Events;

public class UI_SettingsPanel : MonoBehaviour
{
    public UnityEvent OnPanelClosed;

    public void Open()
    {
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        OnPanelClosed?.Invoke();
    }
}
