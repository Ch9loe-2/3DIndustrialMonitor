using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class SettingsPanel : MonoBehaviour
{
    [Header("设置 UI")]
    [SerializeField] private TMP_InputField apiAddressInput;
    [SerializeField] private TMP_Text connectionStatusText;
    [SerializeField] private GameObject testConnectionButton;

    private const string ApiAddressPrefKey = "ApiAddress";

    private void Start()
    {
        // 读取已保存的 API 地址
        string savedAddress = PlayerPrefs.GetString(ApiAddressPrefKey, "http://localhost:5000");
        if (apiAddressInput != null)
        {
            apiAddressInput.text = savedAddress;
        }

        // 尝试自动连接
        TryAutoConnect(savedAddress);
    }

    private async void TryAutoConnect(string address)
    {
        // 等待一帧确保 ApiClient 已初始化
        await Task.Yield();

        if (ApiClient.Instance == null)
        {
            UpdateConnectionStatus(false);
            return;
        }

        ApiClient.Instance.BaseUrl = address;

        // 订阅连接状态变化
        ApiClient.Instance.OnConnectionStatusChanged += OnApiConnectionChanged;

        bool connected = await ApiClient.Instance.TestConnection();
        UpdateConnectionStatus(connected);
    }

    /// <summary>点击"测试连接"按钮时调用</summary>
    public void OnTestConnection()
    {
        string address = apiAddressInput != null ? apiAddressInput.text : "http://localhost:5000";

        // 保存地址
        PlayerPrefs.SetString(ApiAddressPrefKey, address);
        PlayerPrefs.Save();

        // 显示测试中状态
        if (connectionStatusText != null)
        {
            connectionStatusText.text = "● 测试中...";
            connectionStatusText.color = Color.yellow;
        }

        if (ApiClient.Instance == null)
        {
            Debug.LogWarning("SettingsPanel: ApiClient 未初始化");
            UpdateConnectionStatus(false);
            return;
        }

        ApiClient.Instance.BaseUrl = address;
        _ = TestConnectionAsync();
    }

    private async Task TestConnectionAsync()
    {
        bool connected = await ApiClient.Instance.TestConnection();
        UpdateConnectionStatus(connected);
    }

    private void OnApiConnectionChanged(bool isConnected)
    {
        UpdateConnectionStatus(isConnected);
    }

    /// <summary>供外部调用：更新连接状态显示</summary>
    public void UpdateConnectionStatus(bool isConnected)
    {
        if (connectionStatusText == null)
        {
            return;
        }

        if (isConnected)
        {
            connectionStatusText.text = "● 已连接";
            connectionStatusText.color = Color.green;
        }
        else
        {
            connectionStatusText.text = "● 未连接";
            connectionStatusText.color = Color.red;
        }
    }

    private void OnDestroy()
    {
        if (ApiClient.Instance != null)
        {
            ApiClient.Instance.OnConnectionStatusChanged -= OnApiConnectionChanged;
        }
    }
}