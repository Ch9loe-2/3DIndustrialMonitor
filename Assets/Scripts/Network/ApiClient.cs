using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// API 客户端单例：管理 Unity 与后端 ASP.NET Core API 的 HTTP 通信。
/// 其他脚本通过 ApiClient.Instance 调用。
/// </summary>
public class ApiClient : MonoBehaviour
{
    private static ApiClient _instance;
    public static ApiClient Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ApiClient");
                _instance = go.AddComponent<ApiClient>();
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    /// <summary>后端 API 基地址，由 SettingsPanel 设置</summary>
    public string BaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>连接状态变化事件</summary>
    public event Action<bool> OnConnectionStatusChanged;

    /// <summary>当前是否已连接</summary>
    public bool IsConnected { get; private set; } = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>测试 API 连接（GET /api/devices）</summary>
    public async Task<bool> TestConnection()
    {
        try
        {
            using var request = UnityWebRequest.Get($"{BaseUrl}/api/devices");
            request.timeout = 5;

            var op = request.SendWebRequest();

            // 等待完成（Task 方式兼容 Unity）
            while (!op.isDone) await Task.Yield();

            bool success = request.result == UnityWebRequest.Result.Success;
            IsConnected = success;

            if (OnConnectionStatusChanged != null)
            {
                OnConnectionStatusChanged(success);
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ApiClient: 连接测试失败 - {e.Message}");
            IsConnected = false;

            if (OnConnectionStatusChanged != null)
            {
                OnConnectionStatusChanged(false);
            }

            return false;
        }
    }

    /// <summary>GET 请求，返回 JSON 字符串</summary>
    public async Task<string> GetAsync(string endpoint)
    {
        try
        {
            using var request = UnityWebRequest.Get($"{BaseUrl}{endpoint}");
            request.timeout = 10;

            var op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }

            Debug.LogWarning($"ApiClient GET {endpoint} 失败: {request.error}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ApiClient GET {endpoint} 异常: {e.Message}");
            return null;
        }
    }

    /// <summary>POST 请求，发送 JSON body</summary>
    public async Task<string> PostAsync(string endpoint, string jsonBody)
    {
        try
        {
            using var request = new UnityWebRequest($"{BaseUrl}{endpoint}", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            var op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }

            Debug.LogWarning($"ApiClient POST {endpoint} 失败: {request.error}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ApiClient POST {endpoint} 异常: {e.Message}");
            return null;
        }
    }

    /// <summary>PUT 请求，发送 JSON body</summary>
    public async Task<string> PutAsync(string endpoint, string jsonBody)
    {
        try
        {
            using var request = new UnityWebRequest($"{BaseUrl}{endpoint}", "PUT");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            var op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }

            Debug.LogWarning($"ApiClient PUT {endpoint} 失败: {request.error}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ApiClient PUT {endpoint} 异常: {e.Message}");
            return null;
        }
    }
}