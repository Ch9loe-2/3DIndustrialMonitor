using UnityEngine;

public class DeviceClick : MonoBehaviour
{
    [SerializeField] private GameObject deviceDetailPanel;

    private void OnMouseDown()
    {
        Debug.Log("点击了设备 A");

        if (deviceDetailPanel != null)
        {
            deviceDetailPanel.SetActive(true);
        }
    }
}