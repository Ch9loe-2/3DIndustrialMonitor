# 三维工业设备监控与故障模拟系统

基于 **Unity + C#** 开发的三维工业设备监控软件，配合 **ASP.NET Core + SQLite** 后端，实现设备状态监控、故障模拟、报警记录与数据持久化的完整闭环。

定位是**工业监控软件**（而非游戏 HUD）：系统概览、设备详情、故障模拟、报警记录、系统设置五大部分，模拟真实数字化生产管理系统的交互逻辑。

---

## 功能演示流程

```
启动系统
   ↓
三维工业车间（设备 A / B / C）
   ↓
点击设备 → 选中并高亮
   ↓
设备详情面板（温度/压力/转速/运行时长/状态）
   ↓
故障模拟（温度过高 / 压力异常）
   ↓
数据逐级变化 → 状态灯 绿→黄→红 → 生成报警
   ↓
报警记录页 + 底部最近报警栏
   ↓
恢复正常 → 数据复位 → 报警标记"已恢复"
   ↓
全过程数据同步至后端 SQLite
```

---

## 技术栈

**前端**：Unity 2022 + C# + uGUI + TextMeshPro

**后端**：ASP.NET Core 10 + Entity Framework Core + SQLite

**通信**：UnityWebRequest + RESTful API + JSON

**架构**：事件驱动（观察者模式）、分层目录结构

---

## 快速开始

### 1. 启动后端

```bash
cd ../IndustrialMonitorAPI
dotnet run --urls "http://localhost:5000"
```

后端会自动创建 SQLite 数据库并写入 3 台设备种子数据。

### 2. 启动前端

Unity 打开本项目，加载 `Assets/Scenes/SampleScene`，点击 Play。

### 3. 验证联调

点击底部导航「系统设置」→「测试连接」，绿色 `● 已连接` 表示前后端联通。

> 若不启动后端，前端仍可独立运行（本地模拟），仅设置页显示"未连接"。

---

## 功能模块

### ① 监控主页
- 三维工业车间，3 台设备带状态灯（绿=正常 / 黄=警告 / 红=故障）
- 鼠标点击设备选中，弹出详情面板
- 右侧系统概览：设备总数、正常/警告/故障/离线实时统计（事件驱动刷新，非每帧轮询）

### ② 设备详情
- 实时数据：温度、压力、转速、运行时长
- 设备状态与状态灯联动
- 故障模拟按钮：温度过高、压力异常、恢复正常

### ③ 故障模拟链路

以温度过高为例：

```
温度  65.2 → 75 → 85 → 95 → 105 ℃
状态  正常 → 警告 → 警告 → 故障 → 故障
状态灯  🟢  →  🟡  →  🟡  →  🔴  →  🔴
报警          （达到 95℃ 时触发）
```

### ④ 报警记录
- 报警记录页：时间 / 设备 / 类型 / 状态 四列
- 状态区分「未恢复」与「已恢复」，恢复时自动回填恢复时间
- 底部最近报警栏：显示最新未恢复报警，无报警时显示 `✓ 当前无异常报警`

### ⑤ 系统设置
- API 地址配置（PlayerPrefs 持久化）
- 连接状态实时显示（未连接 / 测试中 / 已连接）
- 测试连接按钮，真连后端验证

---

## 项目结构

```
Assets/
├── Editor/
│   ├── SetupIndustrialMonitorUI.cs    # 一键装配 UI 工具
│   └── ReorganizeScripts.cs            # 一键整理目录工具
├── Scripts/
│   ├── Core/
│   │   └── SystemEvents.cs             # 事件总线（观察者模式）
│   ├── Equipment/
│   │   ├── DeviceData.cs               # 设备数据模型
│   │   ├── DeviceController.cs         # 设备交互 + 故障模拟 + API 同步
│   │   └── DeviceClick.cs              # 点击检测
│   ├── UI/
│   │   ├── MonitoringOverview.cs       # 系统概览（事件驱动）
│   │   ├── PanelSwitcher.cs            # 多面板切换
│   │   └── SettingsPanel.cs            # 设置页 + 连接测试
│   ├── Simulation/
│   │   └── AlarmManager.cs             # 报警管理 + API 同步
│   ├── Data/
│   │   └── AlarmRecord.cs              # 报警数据模型
│   └── Network/
│       └── ApiClient.cs                # HTTP 客户端单例
├── Prefabs/
├── Scenes/
│   └── SampleScene.unity
└── Fonts/
```

---

## 架构设计

### 事件驱动解耦

各模块不直接相互引用，通过 `SystemEvents` 事件总线通信：

```csharp
// 设备状态变化 → 广播
SystemEvents.RaiseDeviceStatusChanged();

// 概览面板订阅 → 自动刷新
SystemEvents.DeviceStatusChanged += UpdateOverview;
```

替代了原本每帧 `Update()` 轮询统计的方式，避免无谓性能开销。

### 分层职责

| 层 | 职责 |
|----|------|
| Core | 基础设施（事件总线） |
| Equipment | 设备实体与交互逻辑 |
| UI | 界面展示与面板切换 |
| Simulation | 故障模拟与报警管理 |
| Data | 数据模型 |
| Network | HTTP 通信 |

### 前后端数据流

```
Unity (ApiClient)
   ↓ UnityWebRequest + JSON
ASP.NET Core API
   ↓ Entity Framework Core
SQLite
```

---

## 编辑器工具

Unity 菜单 `Tools` 下提供两个自动化脚本：

| 菜单项 | 作用 |
|--------|------|
| `Setup Industrial Monitor UI V1` | 一键创建设置页、底部导航栏、最近报警栏等 UI 并自动绑定引用与事件 |
| `Reorganize Scripts Directory` | 一键将脚本按 Core/Equipment/UI/Simulation/Data/Network 分层整理 |

> 使用前提：需在非 Play 模式下运行（脚本已做防呆检查）。

---

## 后端仓库

后端服务独立仓库：[IndustrialMonitorAPI](../IndustrialMonitorAPI)

包含设备管理、报警记录的 REST API 及 SQLite 持久化，详见其 README。

---

## 开发计划

- [x] 三维车间与设备建模
- [x] 设备选中与详情面板
- [x] 故障模拟与状态联动
- [x] 报警记录闭环（未恢复 / 已恢复）
- [x] 系统概览实时统计
- [x] ASP.NET Core 后端 + SQLite
- [x] Unity ↔ API 联调
- [ ] 历史数据折线图
- [ ] 设备离线检测
- [ ] 数据报表导出
