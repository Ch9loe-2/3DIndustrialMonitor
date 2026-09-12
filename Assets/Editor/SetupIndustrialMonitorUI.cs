using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using System.Linq;

public class SetupIndustrialMonitorUI
{
    [MenuItem("Tools/Setup Industrial Monitor UI V1")]
    public static void Setup()
    {
        // 防呆：Play 模式不许改场景
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "请先停止 Play 模式再运行此工具。", "确定");
            return;
        }

        Undo.IncrementCurrentGroup();
        int groupIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup V1 Industrial Monitor UI");

        try
        {
            // ============================================================
            // 0. 找到关键对象
            // ============================================================
            GameObject canvasGO = GameObject.Find("MonitoringCanvas");
            if (canvasGO == null)
            {
                EditorUtility.DisplayDialog("错误", "场景中找不到 MonitoringCanvas！请确保场景已加载。", "确定");
                return;
            }

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("错误", "MonitoringCanvas 缺少 Canvas 组件！", "确定");
                return;
            }

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();

            PanelSwitcher switcher = canvasGO.GetComponent<PanelSwitcher>();
            if (switcher == null)
            {
                EditorUtility.DisplayDialog("错误", "MonitoringCanvas 缺少 PanelSwitcher 组件！", "确定");
                return;
            }

            MonitoringOverview overview = canvasGO.GetComponent<MonitoringOverview>();

            GameObject alarmMgrGO = GameObject.Find("AlarmManager");
            AlarmManager alarmMgr = alarmMgrGO != null ? alarmMgrGO.GetComponent<AlarmManager>() : null;

            // 加载项目中已有的字体（与现有文字保持一致）
            TMP_FontAsset fontAsset = null;
            string fontPath = AssetDatabase.GUIDToAssetPath("cdfa7aa6581984ded8e32df07bb0c59e");
            if (!string.IsNullOrEmpty(fontPath))
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }

            // ============================================================
            // A. 概览面板 — 加"离线"项
            // ============================================================
            Transform overviewPanel = canvasGO.transform.Find("OverviewPanel");
            if (overviewPanel != null)
            {
                AddOfflineItem(overviewPanel, overview, fontAsset);
            }

            // ============================================================
            // B. 底部"最近报警"栏
            // ============================================================
            AddRecentAlarmBar(canvasGO.transform, alarmMgr, fontAsset);

            // ============================================================
            // C. 系统设置页
            // ============================================================
            AddSettingsPanel(canvasGO.transform, switcher, fontAsset);

            // ============================================================
            // D. 底部导航栏
            // ============================================================
            AddBottomNavBar(canvasGO.transform, switcher, fontAsset);

            // ============================================================
            // E1. 历史数据页（折线图）+ 采样器
            // ============================================================
            AddHistoryRecorder();
            AddHistoryPanel(canvasGO.transform, switcher, fontAsset);

            // ============================================================
            // E2. TopBar 状态联动（API 状态 + 系统状态）
            // ============================================================
            AddTopBarController(canvasGO.transform);

            // ============================================================
            // E3. 设置页 — 加数据报表导出按钮
            // ============================================================
            AddExportButtons(canvasGO.transform, fontAsset);

            // ============================================================
            // E4. 设备详情面板 — 加"离线/上线"按钮
            // ============================================================
            AddOfflineButton(canvasGO.transform, fontAsset);

            // ============================================================
            // F. 删除旧的 AlarmButton
            // ============================================================
            Transform topBar = canvasGO.transform.Find("TopBar");
            if (topBar != null)
            {
                Transform oldBtn = topBar.Find("AlarmButton");
                if (oldBtn != null)
                {
                    Undo.DestroyObjectImmediate(oldBtn.gameObject);
                }
            }

            Undo.CollapseUndoOperations(groupIndex);

            // 标记场景为 dirty 以便保存
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("✅ 完成", "UI 装配完成！\n\n已自动完成：\n• 概览面板「离线」项\n• 底部最近报警栏\n• 系统设置页 + 报表导出\n• 底部导航栏（历史记录已接入）\n• 历史数据页（折线图）\n• 顶部状态栏联动\n• 设备详情「离线/上线」按钮\n\n请 Ctrl+S (Cmd+S) 保存场景。", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UI Setup 出错: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"装配失败：{e.Message}", "确定");
        }
    }

    // ==================================================================
    // 辅助：把 RectTransform 锚点设为"顶部中心"
    // ==================================================================
    private static void SetTopCenterAnchor(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
    }

    // ==================================================================
    // 辅助：创建 TMP Text 对象
    // ==================================================================
    private static TextMeshProUGUI CreateTMPText(Transform parent, string name,
        float x, float y, float w, float h,
        string text, int fontSize, Color color,
        TextAlignmentOptions alignment,
        TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.up;    // (0,1)
        rt.anchorMax = Vector2.up;    // (0,1)
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        go.AddComponent<CanvasRenderer>();

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;

        if (font != null)
        {
            tmp.font = font;
        }

        return tmp;
    }

    private static TextMeshProUGUI CreateTMPText(Transform parent, string name,
        float x, float y, float w, float h,
        string text, int fontSize, Color color,
        TMP_FontAsset font)
    {
        return CreateTMPText(parent, name, x, y, w, h, text, fontSize, color,
            TextAlignmentOptions.TopLeft, font);
    }

    // ==================================================================
    // 辅助：创建 Button
    // ==================================================================
    private static Button CreateButton(Transform parent, string name,
        float x, float y, float w, float h,
        string label, int fontSize, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.up;
        rt.anchorMax = Vector2.up;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        go.AddComponent<CanvasRenderer>();

        Image img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.15f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // 子对象：文字
        TextMeshProUGUI tmp = CreateTMPText(go.transform, "Text (TMP)",
            0, 0, w, h, label, fontSize, Color.white,
            TextAlignmentOptions.Center, font);
        // 文字子对象用 stretch 撑满按钮
        RectTransform txtRt = tmp.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.pivot = new Vector2(0.5f, 0.5f);
        txtRt.anchoredPosition = Vector2.zero;
        txtRt.sizeDelta = Vector2.zero;

        return btn;
    }

    // ==================================================================
    // 辅助：创建 Panel（Image 背景）
    // ==================================================================
    private static GameObject CreatePanel(Transform parent, string name,
        float left, float right, float top, float bottom, Color bgColor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);

        go.AddComponent<CanvasRenderer>();

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        return go;
    }

    // ==================================================================
    // 辅助：为 SerializedObject 赋值
    // ==================================================================
    private static void SetSerializedProperty(Object target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }

    // ==================================================================
    // 步骤 A：概览面板加离线项
    // ==================================================================
    private static void AddOfflineItem(Transform overviewPanel, MonitoringOverview overview,
        TMP_FontAsset font)
    {
        // 参考 FaultDevicesLabel 的位置（y=-95），加到它下面
        // 已有 4 行，第 5 行在 y=-120 左右
        TextMeshProUGUI offlineLabel = CreateTMPText(overviewPanel, "OfflineDevicesLabel",
            20, -120, 180, 28,
            "● 离线", 18,
            new Color(0.6f, 0.6f, 0.6f), font);

        TextMeshProUGUI offlineValue = CreateTMPText(overviewPanel, "OfflineDevicesValue",
            180, -120, 40, 28,
            "0", 18,
            new Color(0.6f, 0.6f, 0.6f),
            TextAlignmentOptions.TopRight, font);

        // 挂到 MonitoringOverview.offlineDevicesValue
        if (overview != null)
        {
            SetSerializedProperty(overview, "offlineDevicesValue", offlineValue);
        }
    }

    // ==================================================================
    // 步骤 B：底部最近报警栏
    // ==================================================================
    private static void AddRecentAlarmBar(Transform canvasTransform, AlarmManager alarmMgr,
        TMP_FontAsset font)
    {
        // 防重复：已有则跳过
        Transform existing = canvasTransform.Find("RecentAlarmBar");
        if (existing != null)
        {
            Debug.Log("AddRecentAlarmBar: 已存在，跳过");
            if (alarmMgr != null)
            {
                TMP_Text existingText = existing.GetComponent<TMP_Text>();
                if (existingText != null)
                {
                    SetSerializedProperty(alarmMgr, "recentAlarmText", existingText);
                }
            }
            return;
        }

        TextMeshProUGUI bar = CreateTMPText(canvasTransform, "RecentAlarmBar",
            0, 30, 800, 30,
            "✓ 当前无异常报警", 16,
            new Color(0f, 0.8f, 0f),
            TextAlignmentOptions.Center, font);

        // 用底部居中锚点
        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 80);

        if (alarmMgr != null)
        {
            SetSerializedProperty(alarmMgr, "recentAlarmText", bar);
        }
    }

    // ==================================================================
    // 步骤 C：系统设置页 — 垂直居中布局
    // ==================================================================
    private static void AddSettingsPanel(Transform canvasTransform, PanelSwitcher switcher,
        TMP_FontAsset font)
    {
        Transform existingPanel = canvasTransform.Find("SettingsPanel");
        GameObject panelGO;
        RectTransform panelRT;

        if (existingPanel != null)
        {
            panelGO = existingPanel.gameObject;
            var children = panelGO.transform.Cast<Transform>().ToArray();
            foreach (var child in children)
                Undo.DestroyObjectImmediate(child.gameObject);
        }
        else
        {
            panelGO = new GameObject("SettingsPanel", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(panelGO, "Create SettingsPanel");
            panelGO.transform.SetParent(canvasTransform, false);
        }

        panelRT = panelGO.GetComponent<RectTransform>();
        // 强制重置位置与 AlarmPanel 对齐（右侧面板）
        panelRT.anchorMin = new Vector2(1f, 0f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = new Vector2(-240, -120);
        panelRT.sizeDelta = new Vector2(480, -240);
        // 注意：RectTransform 禁止设置 localPosition，否则覆盖 anchoredPosition

        panelGO.AddComponent<CanvasRenderer>();
        Image bg = panelGO.GetComponent<Image>();
        if (bg == null) bg = panelGO.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.35f);
        panelGO.SetActive(false);

        // ===== 垂直居中布局 =====
        // 注意：所有子元素锚点必须用"顶部中心"(0.5, 1)，坐标 x=0 即面板中线

        // 1. 标题
        TextMeshProUGUI titleTmp = CreateTMPText(panelGO.transform, "SettingsTitle",
            0, -60, 200, 40,
            "系统设置", 26, Color.white,
            TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(titleTmp.GetComponent<RectTransform>());

        // 2. 分隔线
        GameObject lineGO = new GameObject("Separator", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(lineGO, "Create Separator");
        lineGO.transform.SetParent(panelGO.transform, false);
        RectTransform lineRT = lineGO.GetComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0.5f, 1f);
        lineRT.anchorMax = new Vector2(0.5f, 1f);
        lineRT.pivot = new Vector2(0.5f, 0.5f);
        lineRT.anchoredPosition = new Vector2(0, -90);
        lineRT.sizeDelta = new Vector2(420, 1);
        Image lineImg = lineGO.AddComponent<Image>();
        lineImg.color = new Color(1, 1, 1, 0.2f);

        // 3. 标签 "API 地址"
        TextMeshProUGUI labelTmp = CreateTMPText(panelGO.transform, "ApiAddressLabel",
            0, -140, 160, 32,
            "API 地址", 20, new Color(0.85f, 0.85f, 0.85f),
            TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(labelTmp.GetComponent<RectTransform>());

        // 4. 输入框（居中）
        GameObject inputGO = new GameObject("ApiAddressInput", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(inputGO, "Create ApiAddressInput");
        inputGO.transform.SetParent(panelGO.transform, false);
        RectTransform inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.5f, 1f);
        inputRT.anchorMax = new Vector2(0.5f, 1f);
        inputRT.pivot = new Vector2(0.5f, 0.5f);
        inputRT.anchoredPosition = new Vector2(0, -185);
        inputRT.sizeDelta = new Vector2(340, 36);

        inputGO.AddComponent<CanvasRenderer>();
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(1, 1, 1, 0.18f);
        inputBg.type = Image.Type.Sliced;

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        GameObject textAreaGO = new GameObject("Text Area", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(textAreaGO, "Create TextArea");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        RectTransform taRT = textAreaGO.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.sizeDelta = new Vector2(-12, -6);
        taRT.anchoredPosition = Vector2.zero;
        textAreaGO.AddComponent<RectMask2D>();

        TextMeshProUGUI placeholder = CreateTMPText(textAreaGO.transform, "Placeholder",
            6, 0, 0, 0,
            "http://localhost:5000", 16,
            new Color(1, 1, 1, 0.4f),
            TextAlignmentOptions.Left, font);
        placeholder.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        placeholder.GetComponent<RectTransform>().anchorMax = Vector2.one;
        placeholder.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        placeholder.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        TextMeshProUGUI inputText = CreateTMPText(textAreaGO.transform, "Text",
            6, 0, 0, 0,
            "", 16, Color.white,
            TextAlignmentOptions.Left, font);
        inputText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        inputText.GetComponent<RectTransform>().anchorMax = Vector2.one;
        inputText.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        inputText.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        inputField.textViewport = taRT;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.fontAsset = placeholder.font;

        // 5. 连接状态 + 测试按钮（同一行，围绕中线对称）
        TextMeshProUGUI statusText = CreateTMPText(panelGO.transform, "ConnectionStatusText",
            -110, -250, 130, 32,
            "● 未连接", 18, Color.red, font);
        SetTopCenterAnchor(statusText.GetComponent<RectTransform>());

        Button testBtn = CreateButton(panelGO.transform, "TestConnectionButton",
            110, -250, 120, 36,
            "测试连接", 16, font);
        SetTopCenterAnchor(testBtn.GetComponent<RectTransform>());
        Image btnImg = testBtn.GetComponent<Image>();
        if (btnImg != null) btnImg.color = new Color(0.2f, 0.5f, 0.9f, 0.85f);

        // --- 挂 SettingsPanel 脚本 ---
        System.Type settingsType = System.Type.GetType("SettingsPanel, Assembly-CSharp");
        if (settingsType == null)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                settingsType = asm.GetType("SettingsPanel");
                if (settingsType != null) break;
            }
        }

        if (settingsType != null)
        {
            Component existing = panelGO.GetComponent(settingsType);
            if (existing != null) Undo.DestroyObjectImmediate(existing);
            Component settingsComp = panelGO.AddComponent(settingsType);
            SetSerializedProperty(settingsComp, "apiAddressInput", inputField);
            SetSerializedProperty(settingsComp, "connectionStatusText", statusText);
        }

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            testBtn.onClick,
            panelGO.GetComponent<SettingsPanel>().OnTestConnection
        );

        SetSerializedProperty(switcher, "settingsPanel", panelGO);
    }

    // ==================================================================
    // 步骤 F：设备详情面板 — 加"离线/上线"按钮
    // 位置自动跟随"恢复正常"按钮下方，无需硬编码坐标
    // ==================================================================
    private static void AddOfflineButton(Transform canvasTransform, TMP_FontAsset font)
    {
        // 找设备详情面板
        Transform detailPanel = canvasTransform.Find("DeviceDetailPanel");
        if (detailPanel == null)
        {
            Debug.LogWarning("AddOfflineButton: 找不到 DeviceDetailPanel");
            return;
        }

        // 已存在则先删除，避免重复
        Transform existing = detailPanel.Find("OfflineButton");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // 找"恢复正常"按钮作为位置参考
        Transform refBtn = detailPanel.Find("RecoveryButton");
        float posX = 0;
        float posY = -300;
        float w = 200;
        float h = 35;
        Vector2 anchorMin = Vector2.up;
        Vector2 anchorMax = Vector2.up;

        if (refBtn != null)
        {
            RectTransform refRT = refBtn.GetComponent<RectTransform>();
            posX = refRT.anchoredPosition.x;
            posY = refRT.anchoredPosition.y - 50;   // 下方 50px
            w = refRT.sizeDelta.x;
            h = refRT.sizeDelta.y;
            anchorMin = refRT.anchorMin;
            anchorMax = refRT.anchorMax;
        }

        // 创建按钮
        Button offlineBtn = CreateButton(detailPanel, "OfflineButton",
            posX, posY, w, h,
            "离线 / 上线", 16, font);

        // 与新按钮保持同样的锚点
        RectTransform btnRT = offlineBtn.GetComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.anchoredPosition = new Vector2(posX, posY);

        // 按钮配色（灰蓝，区别于故障按钮）
        Image btnImg = offlineBtn.GetComponent<Image>();
        if (btnImg != null)
        {
            btnImg.color = new Color(0.35f, 0.35f, 0.4f, 0.85f);
        }

        // 把按钮引用赋给场景中所有 DeviceController
        // （DeviceController.OnMouseDown 里会动态绑定点击回调到当前选中设备）
        DeviceController[] controllers = UnityEngine.Object.FindObjectsOfType<DeviceController>(true);
        foreach (DeviceController ctrl in controllers)
        {
            SetSerializedProperty(ctrl, "offlineButton", offlineBtn);
        }

        Debug.Log($"AddOfflineButton: 已创建，并绑定到 {controllers.Length} 个 DeviceController");
    }

    // ==================================================================
    // 步骤 D：底部导航栏
    // ==================================================================
    private static void AddBottomNavBar(Transform canvasTransform, PanelSwitcher switcher,
        TMP_FontAsset font)
    {
        // 查找是否已有 BottomNavBar
        Transform existing = canvasTransform.Find("BottomNavBar");
        GameObject navGO;

        if (existing != null)
        {
            navGO = existing.gameObject;
            // 删除旧子对象
            var children = navGO.transform.Cast<Transform>().ToArray();
            foreach (var child in children)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
        else
        {
            navGO = new GameObject("BottomNavBar", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(navGO, "Create BottomNavBar");
            navGO.transform.SetParent(canvasTransform, false);
        }

        RectTransform rt = navGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 52);
        rt.sizeDelta = new Vector2(900, 40);

        navGO.AddComponent<CanvasRenderer>();
        Image bg = navGO.GetComponent<Image>();
        if (bg == null) bg = navGO.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.08f);

        // 创建 5 个按钮
        string[] btnNames = { "Btn_DeviceList", "Btn_Monitor", "Btn_AlarmRecord", "Btn_History", "Btn_Settings" };
        string[] btnLabels = { "设备列表", "实时监控", "报警记录", "历史记录", "系统设置" };
        float[] btnPosX = { -360, -180, 0, 180, 360 };
        string[] switchMethods = { "ShowOverview", "", "ShowAlarmPanel", "ShowHistoryPanel", "ShowSettingsPanel" };

        for (int i = 0; i < 5; i++)
        {
            Button btn = CreateButton(navGO.transform, btnNames[i],
                btnPosX[i], 0, 140, 32,
                btnLabels[i], 15, font);

            // 绑定点击事件
            if (!string.IsNullOrEmpty(switchMethods[i]) && switcher != null)
            {
                System.Type switcherType = switcher.GetType();
                System.Reflection.MethodInfo method = switcherType.GetMethod(switchMethods[i],
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(
                        btn.onClick,
                        (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction), switcher, method)
                    );
                }
            }
        }
    }

    // ==================================================================
    // 步骤 E2：TopBar 状态联动控制器
    // 让顶部"系统状态"和"API 状态"跟随真实数据变化，而非写死
    // ==================================================================
    private static void AddTopBarController(Transform canvasTransform)
    {
        Transform topBar = canvasTransform.Find("TopBar");
        if (topBar == null)
        {
            Debug.LogWarning("AddTopBarController: 找不到 TopBar");
            return;
        }

        // 移除旧组件，避免重复叠加
        TopBarController oldCtrl = topBar.GetComponent<TopBarController>();
        if (oldCtrl != null)
        {
            Undo.DestroyObjectImmediate(oldCtrl);
        }

        TopBarController ctrl = topBar.gameObject.AddComponent<TopBarController>();

        // 绑定"系统状态"文字
        Transform sysText = topBar.Find("SystemStatusText");
        if (sysText != null)
        {
            SetSerializedProperty(ctrl, "systemStatusText", sysText.GetComponent<TMP_Text>());
        }
        else
        {
            Debug.LogWarning("AddTopBarController: 找不到 SystemStatusText");
        }

        // 绑定"API 状态"文字
        Transform apiText = topBar.Find("ApiStatusText");
        if (apiText != null)
        {
            SetSerializedProperty(ctrl, "apiStatusText", apiText.GetComponent<TMP_Text>());
        }
        else
        {
            Debug.LogWarning("AddTopBarController: 找不到 ApiStatusText");
        }

        Debug.Log("AddTopBarController: 顶部状态栏已接入联动");
    }

    // ==================================================================
    // 步骤 E3：设置页 — 数据报表导出按钮
    // ==================================================================
    private static void AddExportButtons(Transform canvasTransform, TMP_FontAsset font)
    {
        Transform settingsPanel = canvasTransform.Find("SettingsPanel");
        if (settingsPanel == null)
        {
            Debug.LogWarning("AddExportButtons: 找不到 SettingsPanel");
            return;
        }

        // 清理旧按钮，避免重复
        string[] oldNames = { "ExportAlarmButton", "ExportDeviceButton" };
        foreach (string oldName in oldNames)
        {
            Transform old = settingsPanel.Find(oldName);
            if (old != null)
            {
                Undo.DestroyObjectImmediate(old.gameObject);
            }
        }

        // 两个并排的导出按钮（放在"测试连接"下方）
        Button alarmExportBtn = CreateButton(settingsPanel, "ExportAlarmButton",
            -75, -320, 140, 34, "导出报警 CSV", 14, font);
        SetTopCenterAnchor(alarmExportBtn.GetComponent<RectTransform>());

        Button deviceExportBtn = CreateButton(settingsPanel, "ExportDeviceButton",
            75, -320, 140, 34, "导出设备 CSV", 14, font);
        SetTopCenterAnchor(deviceExportBtn.GetComponent<RectTransform>());

        // 配色（绿色系，区别于蓝色测试按钮）
        Image img1 = alarmExportBtn.GetComponent<Image>();
        if (img1 != null) img1.color = new Color(0.2f, 0.6f, 0.35f, 0.85f);

        Image img2 = deviceExportBtn.GetComponent<Image>();
        if (img2 != null) img2.color = new Color(0.2f, 0.6f, 0.35f, 0.85f);

        // 挂 ReportExporter 脚本
        ReportExporter exporter = settingsPanel.GetComponent<ReportExporter>();
        if (exporter == null)
        {
            exporter = settingsPanel.gameObject.AddComponent<ReportExporter>();
        }

        // 绑定点击事件
        BindMethod(alarmExportBtn, exporter, "ExportAlarms");
        BindMethod(deviceExportBtn, exporter, "ExportDevices");

        Debug.Log("AddExportButtons: 导出按钮创建完成");
    }

    /// <summary>把按钮的 onClick 绑定到目标对象的指定无参方法</summary>
    private static void BindMethod(Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;

        var method = target.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (method == null)
        {
            Debug.LogWarning($"BindMethod: 找不到方法 {methodName}");
            return;
        }

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
        );
    }

    // ==================================================================
    // 步骤 E1：创建历史数据采样器（场景中的逻辑对象）
    // ==================================================================
    private static void AddHistoryRecorder()
    {
        // 已存在则跳过，避免重复
        if (UnityEngine.Object.FindObjectOfType<HistoryRecorder>() != null)
        {
            return;
        }

        GameObject go = new GameObject("HistoryRecorder");
        Undo.RegisterCreatedObjectUndo(go, "Create HistoryRecorder");
        go.AddComponent<HistoryRecorder>();

        Debug.Log("AddHistoryRecorder: 已创建采样器");
    }

    // ==================================================================
    // 步骤 E2：历史数据页面（折线图 + 设备/指标切换）
    // ==================================================================
    private static void AddHistoryPanel(Transform canvasTransform, PanelSwitcher switcher,
        TMP_FontAsset font)
    {
        Transform existingPanel = canvasTransform.Find("HistoryPanel");
        GameObject panelGO;

        if (existingPanel != null)
        {
            panelGO = existingPanel.gameObject;
            var children = panelGO.transform.Cast<Transform>().ToArray();
            foreach (var child in children)
                Undo.DestroyObjectImmediate(child.gameObject);
        }
        else
        {
            panelGO = new GameObject("HistoryPanel", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(panelGO, "Create HistoryPanel");
            panelGO.transform.SetParent(canvasTransform, false);
        }

        // 与其他面板一致的右侧布局
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 0f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = new Vector2(-240, -120);
        panelRT.sizeDelta = new Vector2(480, -240);

        panelGO.AddComponent<CanvasRenderer>();
        Image bg = panelGO.GetComponent<Image>();
        if (bg == null) bg = panelGO.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.1f);

        // ===== 子元素（垂直居中，锚点统一顶部中心）=====

        // 1. 标题
        TextMeshProUGUI title = CreateTMPText(panelGO.transform, "HistoryTitle",
            0, -50, 240, 36,
            "历史数据", 24, Color.white,
            TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(title.GetComponent<RectTransform>());

        // 2. 设备选择按钮（3 个）
        string[] devNames = { "Btn_DevA", "Btn_DevB", "Btn_DevC" };
        string[] devLabels = { "设备 A", "设备 B", "设备 C" };
        float[] devX = { -130, 0, 130 };
        string[] devMethods = { "SelectDeviceA", "SelectDeviceB", "SelectDeviceC" };
        Button[] devButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            devButtons[i] = CreateButton(panelGO.transform, devNames[i],
                devX[i], -100, 110, 30, devLabels[i], 14, font);
            SetTopCenterAnchor(devButtons[i].GetComponent<RectTransform>());
        }

        // 3. 指标选择按钮（2 个）
        Button tempBtn = CreateButton(panelGO.transform, "Btn_Temperature",
            -65, -145, 110, 30, "温度", 14, font);
        SetTopCenterAnchor(tempBtn.GetComponent<RectTransform>());

        Button pressBtn = CreateButton(panelGO.transform, "Btn_Pressure",
            65, -145, 110, 30, "压力", 14, font);
        SetTopCenterAnchor(pressBtn.GetComponent<RectTransform>());

        // 4. 折线图区域
        GameObject chartGO = new GameObject("LineChart", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(chartGO, "Create LineChart");
        chartGO.transform.SetParent(panelGO.transform, false);
        RectTransform chartRT = chartGO.GetComponent<RectTransform>();
        chartRT.anchorMin = new Vector2(0.5f, 1f);
        chartRT.anchorMax = new Vector2(0.5f, 1f);
        chartRT.pivot = new Vector2(0.5f, 0.5f);
        chartRT.anchoredPosition = new Vector2(0, -290);
        chartRT.sizeDelta = new Vector2(400, 190);

        chartGO.AddComponent<CanvasRenderer>();
        LineChart chart = chartGO.AddComponent<LineChart>();

        // 5. 统计信息文字
        TextMeshProUGUI stat = CreateTMPText(panelGO.transform, "StatText",
            0, -420, 440, 30,
            "暂无数据（等待采样中...）", 15, new Color(0.85f, 0.85f, 0.85f),
            TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(stat.GetComponent<RectTransform>());

        panelGO.SetActive(false);

        // ===== 挂 HistoryPanel 脚本并绑定引用 =====
        HistoryPanel historyPanel = panelGO.GetComponent<HistoryPanel>();
        if (historyPanel == null)
        {
            historyPanel = panelGO.AddComponent<HistoryPanel>();
        }

        SetSerializedProperty(historyPanel, "lineChart", chart);
        SetSerializedProperty(historyPanel, "titleText", title);
        SetSerializedProperty(historyPanel, "statText", stat);

        // 绑定设备切换按钮
        for (int i = 0; i < 3; i++)
        {
            int index = i;  // 闭包捕获
            var method = typeof(HistoryPanel).GetMethod(devMethods[index],
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    devButtons[index].onClick,
                    (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                        typeof(UnityEngine.Events.UnityAction), historyPanel, method)
                );
            }
        }

        // 绑定指标切换按钮
        var tempMethod = typeof(HistoryPanel).GetMethod("SelectTemperature",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (tempMethod != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                tempBtn.onClick,
                (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), historyPanel, tempMethod)
            );
        }

        var pressMethod = typeof(HistoryPanel).GetMethod("SelectPressure",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (pressMethod != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                pressBtn.onClick,
                (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), historyPanel, pressMethod)
            );
        }

        // 挂到 PanelSwitcher
        SetSerializedProperty(switcher, "historyPanel", panelGO);

        Debug.Log("AddHistoryPanel: 历史数据页创建完成");
    }

    // ==================================================================
    // 一键装配：多车间物理分区 + 概览车间统计 + 历史页车间按钮 + 登录 UI
    // 需先运行 Tools/Setup Industrial Monitor UI V1（生成 OverviewPanel/HistoryPanel/SettingsPanel）
    // ==================================================================
    [MenuItem("Tools/一键装配：多车间与登录UI")]
    public static void OneClickMultiWorkshopSetup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "请先停止 Play 模式再运行此工具。", "确定");
            return;
        }

        Undo.IncrementCurrentGroup();
        int groupIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("一键装配 多车间+登录UI");

        try
        {
            GameObject canvasGO = GameObject.Find("MonitoringCanvas");
            if (canvasGO == null)
            {
                EditorUtility.DisplayDialog("错误",
                    "场景中找不到 MonitoringCanvas！请先运行 Tools/Setup Industrial Monitor UI V1。", "确定");
                return;
            }

            TMP_FontAsset fontAsset = null;
            string fontPath = AssetDatabase.GUIDToAssetPath("cdfa7aa6581984ded8e32df07bb0c59e");
            if (!string.IsNullOrEmpty(fontPath))
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }

            // 1. 复制二号车间（物理分区：外壳 + 设备 C）
            SetupWorkshops.CopySecondWorkshop();

            // 2. 概览面板：车间分组统计文本
            MonitoringOverview overview = canvasGO.GetComponent<MonitoringOverview>();
            BindWorkshopSummary(canvasGO, overview, fontAsset);

            // 3. 历史页：车间切换按钮
            Transform hp = canvasGO.transform.Find("HistoryPanel");
            HistoryPanel historyPanel = hp != null ? hp.GetComponent<HistoryPanel>() : null;
            BindWorkshopButtons(historyPanel, fontAsset);

            // 4. 设置页：登录 UI
            Transform sp = canvasGO.transform.Find("SettingsPanel");
            BindLoginUI(sp != null ? sp.gameObject : null, fontAsset);

            Undo.CollapseUndoOperations(groupIndex);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("✅ 完成",
                "一键装配完成！\n\n" +
                "• 二号车间已复制（设备 C 已移入）\n" +
                "• 概览面板已显示车间分组统计\n" +
                "• 历史页已加入「一号/二号车间」切换按钮\n" +
                "• 设置页已加入登录区（用户名 / 密码 / 登录）\n\n" +
                "请 Ctrl+S (Cmd+S) 保存场景。", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"一键装配出错: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"装配失败：{e.Message}", "确定");
        }
    }

    // ==================================================================
    // 概览面板：车间分组统计文本（绑定 MonitoringOverview.workshopSummaryText）
    // ==================================================================
    private static void BindWorkshopSummary(GameObject canvasGO, MonitoringOverview overview,
        TMP_FontAsset font)
    {
        Transform overviewPanel = canvasGO.transform.Find("OverviewPanel");
        if (overviewPanel == null)
        {
            Debug.LogWarning("BindWorkshopSummary: 找不到 OverviewPanel");
            return;
        }

        if (overviewPanel.Find("WorkshopSummaryText") != null)
        {
            Debug.Log("BindWorkshopSummary: 已存在，跳过");
            return;
        }

        TextMeshProUGUI ws = CreateTMPText(overviewPanel, "WorkshopSummaryText",
            20, -155, 360, 28,
            "车间统计：", 16, new Color(0.7f, 0.85f, 1f), font);

        if (overview != null)
        {
            SetSerializedProperty(overview, "workshopSummaryText", ws);
        }

        Debug.Log("BindWorkshopSummary: 车间统计文本已绑定");
    }

    // ==================================================================
    // 历史页：车间切换按钮（绑定 HistoryPanel.SelectWorkshop1/2）
    // ==================================================================
    private static void BindWorkshopButtons(HistoryPanel historyPanel, TMP_FontAsset font)
    {
        if (historyPanel == null)
        {
            Debug.LogWarning("BindWorkshopButtons: 找不到 HistoryPanel，请先运行 V1 Setup");
            return;
        }

        Transform hp = historyPanel.transform;
        if (hp.Find("Btn_Workshop1") != null)
        {
            Debug.Log("BindWorkshopButtons: 已存在，跳过");
            return;
        }

        Button w1 = CreateButton(hp, "Btn_Workshop1", -65, -185, 120, 30, "一号车间", 14, font);
        SetTopCenterAnchor(w1.GetComponent<RectTransform>());

        Button w2 = CreateButton(hp, "Btn_Workshop2", 65, -185, 120, 30, "二号车间", 14, font);
        SetTopCenterAnchor(w2.GetComponent<RectTransform>());

        BindMethod(w1, historyPanel, "SelectWorkshop1");
        BindMethod(w2, historyPanel, "SelectWorkshop2");

        Debug.Log("BindWorkshopButtons: 车间切换按钮已绑定");
    }

    // ==================================================================
    // 设置页：登录 UI（用户名 / 密码 / 登录按钮 / 当前用户 / 登录状态）
    // 绑定到 SettingsPanel 的登录字段
    // ==================================================================
    private static void BindLoginUI(GameObject spGO, TMP_FontAsset font)
    {
        if (spGO == null)
        {
            Debug.LogWarning("BindLoginUI: 找不到 SettingsPanel，请先运行 V1 Setup");
            return;
        }

        if (spGO.transform.Find("LoginUserInput") != null)
        {
            Debug.Log("BindLoginUI: 登录 UI 已存在，跳过");
            return;
        }

        // 用户名输入
        TMP_InputField userInput = CreateInputField(spGO.transform, "LoginUserInput",
            0, -360, 240, 34, "用户名", font);
        SetTopCenterAnchor(userInput.GetComponent<RectTransform>());

        // 密码输入
        TMP_InputField passInput = CreateInputField(spGO.transform, "LoginPassInput",
            0, -405, 240, 34, "密码", font);
        SetTopCenterAnchor(passInput.GetComponent<RectTransform>());
        passInput.contentType = TMP_InputField.ContentType.Password;

        // 登录按钮
        Button loginBtn = CreateButton(spGO.transform, "LoginButton",
            0, -450, 160, 34, "登录", 16, font);
        SetTopCenterAnchor(loginBtn.GetComponent<RectTransform>());
        Image btnImg = loginBtn.GetComponent<Image>();
        if (btnImg != null) btnImg.color = new Color(0.2f, 0.5f, 0.9f, 0.85f);

        // 当前用户文本
        TextMeshProUGUI curUser = CreateTMPText(spGO.transform, "CurrentUserText",
            0, -490, 260, 28, "当前用户：未登录", 15,
            new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(curUser.GetComponent<RectTransform>());

        // 登录状态文本
        TextMeshProUGUI loginStatus = CreateTMPText(spGO.transform, "LoginStatusText",
            0, -515, 260, 24, "", 13, Color.yellow, TextAlignmentOptions.Center, font);
        SetTopCenterAnchor(loginStatus.GetComponent<RectTransform>());

        // 绑定到 SettingsPanel 组件
        Component settingsComp = spGO.GetComponent("SettingsPanel");
        if (settingsComp != null)
        {
            SetSerializedProperty(settingsComp, "loginUserInput", userInput);
            SetSerializedProperty(settingsComp, "loginPassInput", passInput);
            SetSerializedProperty(settingsComp, "loginButton", loginBtn);
            SetSerializedProperty(settingsComp, "currentUserText", curUser);
            SetSerializedProperty(settingsComp, "loginStatusText", loginStatus);

            BindMethod(loginBtn, settingsComp, "OnLogin");
        }

        Debug.Log("BindLoginUI: 登录 UI 装配完成");
    }

    // ==================================================================
    // 辅助：创建 TMP_InputField（含 Text Area / Placeholder / Text）
    // ==================================================================
    private static TMP_InputField CreateInputField(Transform parent, string name,
        float x, float y, float w, float h, string placeholderText, TMP_FontAsset font)
    {
        GameObject inputGO = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(inputGO, "Create " + name);
        inputGO.transform.SetParent(parent, false);

        RectTransform inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0.5f, 1f);
        inputRT.anchorMax = new Vector2(0.5f, 1f);
        inputRT.pivot = new Vector2(0.5f, 0.5f);
        inputRT.anchoredPosition = new Vector2(x, y);
        inputRT.sizeDelta = new Vector2(w, h);

        inputGO.AddComponent<CanvasRenderer>();
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(1, 1, 1, 0.18f);
        inputBg.type = Image.Type.Sliced;

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        GameObject textAreaGO = new GameObject("Text Area", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(textAreaGO, "Create TextArea");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        RectTransform taRT = textAreaGO.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.sizeDelta = new Vector2(-12, -6);
        taRT.anchoredPosition = Vector2.zero;
        textAreaGO.AddComponent<RectMask2D>();

        TextMeshProUGUI placeholder = CreateTMPText(textAreaGO.transform, "Placeholder",
            6, 0, 0, 0, placeholderText, 16,
            new Color(1, 1, 1, 0.4f), TextAlignmentOptions.Left, font);
        RectTransform phRT = placeholder.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.sizeDelta = Vector2.zero;
        phRT.anchoredPosition = Vector2.zero;

        TextMeshProUGUI inputText = CreateTMPText(textAreaGO.transform, "Text",
            6, 0, 0, 0, "", 16, Color.white, TextAlignmentOptions.Left, font);
        RectTransform itRT = inputText.GetComponent<RectTransform>();
        itRT.anchorMin = Vector2.zero;
        itRT.anchorMax = Vector2.one;
        itRT.sizeDelta = Vector2.zero;
        itRT.anchoredPosition = Vector2.zero;

        inputField.textViewport = taRT;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.fontAsset = placeholder.font;

        return inputField;
    }
}