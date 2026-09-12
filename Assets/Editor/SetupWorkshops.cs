using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 多车间扩展的物理分区工具：一键复制出「二号车间」外壳，并把设备 C 移到二号车间。
/// 在 Unity 编辑器内通过菜单 Tools / Setup Workshops (复制二号车间) 执行。
/// </summary>
public class SetupWorkshops
{
    [MenuItem("Tools/Setup Workshops (复制二号车间)")]
    static void CopySecondWorkshop()
    {
        GameObject floor = GameObject.Find("WorkshopFloor");
        if (floor == null)
        {
            Debug.LogError("未找到 WorkshopFloor，请确认场景已加载且为一号车间外壳。");
            return;
        }

        if (GameObject.Find("WorkshopFloor_2") != null)
        {
            Debug.LogWarning(
                "二号车间已存在，跳过。如需重做请先手动删除 Workshop*_2 外壳并把设备 C 移回一号车间。");
            return;
        }

        // 二号车间相对一号车间沿 X 轴偏移
        Vector3 offset = new Vector3(40f, 0f, 0f);
        string[] shellNames = { "WorkshopFloor", "WorkshopRightWall", "WorkshopBackWall", "WorkshopLeftWall" };

        GameObject floor2 = null;
        foreach (string name in shellNames)
        {
            GameObject src = GameObject.Find(name);
            if (src == null)
            {
                Debug.LogWarning("未找到外壳物体：" + name);
                continue;
            }

            GameObject copy = Object.Instantiate(src);
            copy.name = name + "_2";
            copy.transform.position = src.transform.position + offset;

            if (name == "WorkshopFloor") floor2 = copy;

            Undo.RegisterCreatedObjectUndo(copy, "复制二号车间外壳 " + name);
        }

        if (floor2 == null)
        {
            Debug.LogError("复制二号车间地板失败。");
            return;
        }

        // 将设备 C 移到二号车间（保持与一号车间的相对位置）
        DeviceData[] devices = Object.FindObjectsOfType<DeviceData>();
        foreach (DeviceData d in devices)
        {
            if (d != null && d.deviceName == "设备 C")
            {
                Vector3 rel = d.transform.position - floor.transform.position;
                Undo.RecordObject(d.transform, "移动设备 C 到二号车间");
                d.transform.position = floor2.transform.position + rel;
                Debug.Log("设备 C 已移动到二号车间。");
                break;
            }
        }

        Scene active = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(active);

        Debug.Log(
            "二号车间复制完成：外壳已复制并偏移，设备 C 已移至二号车间。" +
            "记得 Ctrl/Cmd+S 保存场景。物理布局（墙体围合、设备摆放细节）可在场景里微调。");
    }
}
