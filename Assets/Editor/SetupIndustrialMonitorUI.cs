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
            // E. 删除旧的 AlarmButton
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

            EditorUtility.DisplayDialog("✅ 完成", "V1 UI 装配完成！\n\n已自动完成：\n• 概览面板加「离线」项\n• 底部最近报警栏\n• 系统设置页\n• 底部导航栏\n• 移除旧报警按钮\n\n请点击 Ctrl+S (Cmd+S) 保存场景。", "确定");
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
        rt.anchoredPosition = new Vector2(0, 28);

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
        string[] switchMethods = { "ShowOverview", "", "ShowAlarmPanel", "", "ShowSettingsPanel" };

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
}