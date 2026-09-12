using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ReorganizeScripts
{
    [MenuItem("Tools/Reorganize Scripts Directory")]
    public static void Reorganize()
    {
        string assetsPath = Application.dataPath;
        string scrptsPath = Path.Combine(assetsPath, "Scrpts");
        string scriptsPath = Path.Combine(assetsPath, "Scripts");

        if (!Directory.Exists(scrptsPath))
        {
            EditorUtility.DisplayDialog("提示", "Scrpts 目录不存在，可能已整理过。", "确定");
            return;
        }

        Undo.IncrementCurrentGroup();
        int groupIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Reorganize Scripts");

        try
        {
            // 1. 创建 Scripts 子目录
            string[] subDirs = { "Core", "Equipment", "UI", "Simulation", "Data", "Network" };
            foreach (string dir in subDirs)
            {
                string fullPath = Path.Combine(scriptsPath, dir);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }

            // 刷新 AssetDatabase 识别新目录
            AssetDatabase.Refresh();

            // 2. 移动文件：用 AssetDatabase API 安全操作
            // 先定义映射：文件名 → 目标子目录
            var fileMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "SystemEvents.cs", "Core" },
                { "DeviceData.cs", "Equipment" },
                { "DeviceClick.cs", "Equipment" },
                { "DeviceController.cs", "Equipment" },
                { "MonitoringOverview.cs", "UI" },
                { "PanelSwitcher.cs", "UI" },
                { "SettingsPanel.cs", "UI" },
                { "AlarmManager.cs", "Simulation" },
                { "AlarmRecord.cs", "Data" },
            };

            // 获取 Scrpts 目录的 asset 路径
            string scrptsAssetPath = "Assets/Scrpts";
            string scriptsAssetPath = "Assets/Scripts";

            foreach (var kv in fileMap)
            {
                string source = scrptsAssetPath + "/" + kv.Key;
                string destDir = scriptsAssetPath + "/" + kv.Value + "/";
                string dest = destDir + kv.Key;

                // 确保目标目录 Asset 存在（AssetDatabase 刚刷新过）
                if (!AssetDatabase.IsValidFolder(destDir.TrimEnd('/')))
                {
                    AssetDatabase.CreateFolder(scriptsAssetPath, kv.Value);
                }

                string error = AssetDatabase.MoveAsset(source, dest);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"移动 {kv.Key} 时出错: {error}");
                }
                else
                {
                    Debug.Log($"已移动 {kv.Key} → Scripts/{kv.Value}/");
                }
            }

            // 3. 删除空的 Scrpts 目录
            AssetDatabase.MoveAsset(scrptsAssetPath, scriptsAssetPath + "/_OldScrpts");
            AssetDatabase.DeleteAsset(scriptsAssetPath + "/_OldScrpts");

            AssetDatabase.Refresh();

            // 4. 标记场景为已修改
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Undo.CollapseUndoOperations(groupIndex);

            EditorUtility.DisplayDialog("✅ 完成",
                "目录整理完成！\n\n" +
                "Assets/Scripts/\n" +
                "├── Core/\n" +
                "│   └── SystemEvents.cs\n" +
                "├── Equipment/\n" +
                "│   ├── DeviceData.cs\n" +
                "│   ├── DeviceClick.cs\n" +
                "│   └── DeviceController.cs\n" +
                "├── UI/\n" +
                "│   ├── MonitoringOverview.cs\n" +
                "│   ├── PanelSwitcher.cs\n" +
                "│   └── SettingsPanel.cs\n" +
                "├── Simulation/\n" +
                "│   └── AlarmManager.cs\n" +
                "├── Data/\n" +
                "│   └── AlarmRecord.cs\n" +
                "└── Network/\n" +
                "    (预留 API 脚本)\n\n" +
                "请 Ctrl+S (Cmd+S) 保存场景。",
                "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"整理目录出错: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"整理失败：{e.Message}", "确定");
        }
    }
}