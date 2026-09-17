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
cd IndustrialMonitorAPI
dotnet run --urls "http://localhost:5175"
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
- **自动阈值报警**：温度/压力超过可配置阈值时自动生成报警，回到正常后自动恢复
- **多车间组织**：设备归属车间（一号/二号），概览按车间分组统计，历史页可按车间筛选

### ③ 故障模拟链路

以温度过高为例：

```
温度  65.2 → 75 → 85 → 95 → 105 ℃
状态  正常 → 警告 → 警告 → 故障 → 故障
状态灯  🟢  →  🟡  →  🟡  →  🔴  →  🔴
报警          （达到 95℃ 时触发）
```

### ③·b 自动阈值报警（无需手动触发）

每台设备的 `DeviceController` 挂载可配置阈值（温度/压力各 警告 + 故障 两档，默认温度 78/90℃、压力 2.0/2.3 MPa），并带轻量自然仿真：温度/压力围绕初始值浮动，并不定时触发一次升温/升压工况事件。每帧做阈值检测：

- 超过**故障阈值** → 状态置「故障」，自动调用 `AlarmManager.AddAlarm`（限频，避免每帧重复）
- 超过**警告阈值** → 状态置「警告」，自动报警
- 全部回到正常 → 自动调用 `AlarmManager.RecoverAlarm` 恢复该设备报警

手动「故障模拟」按钮仍保留：它把数值拉高，最终由阈值检测统一判定状态与报警，二者不冲突。

### ③·c 多车间组织

设备按车间归属：A/B 在**一号车间**、C 在**二号车间**（`DeviceData.workshop` 字段，Awake 按设备名自动判定，可在 Inspector 覆盖）。

- **数据模型**：后端 `Device` 增加 `Workshop` 字段，种子数据已分配车间
- **概览分组统计**：`MonitoringOverview` 按车间统计各车间设备数 / 异常数（需绑定 `workshopSummaryText`）
- **设备详情**：显示设备所属车间
- **历史页筛选**：`HistoryPanel` 提供 `SelectWorkshop1/2` 按钮，按车间自动切换首台设备
- **后端查询**：`GET /api/devices/workshop/{workshop}` 按车间返回设备
- **物理分区**：执行菜单 `Tools / Setup Workshops (复制二号车间)`，一键复制二号车间外壳（地板+三面墙，沿 X 轴偏移 40）并把设备 C 移到二号车间；布局细节可在场景里微调

### ④ 报警记录
- 报警记录页：时间 / 设备 / 类型 / 状态 四列
- 状态区分「未恢复」与「已恢复」，恢复时自动回填恢复时间
- 底部最近报警栏：显示最新未恢复报警，无报警时显示 `✓ 当前无异常报警`

### ⑤ 系统设置
- API 地址配置（PlayerPrefs 持久化）
- 连接状态实时显示（未连接 / 测试中 / 已连接）
- 测试连接按钮，真连后端验证
- **数据报表导出**：报警记录、设备状态一键导出 CSV

### ⑥ 设备离线检测
- 设备详情面板「离线 / 上线」按钮切换在线状态
- 离线时状态灯变灰，系统概览「离线」计数 +1
- 恢复上线自动还原离线前的状态（不丢失警告/故障）
- 顶部状态栏同步显示「设备离线（N）」

### ⑦ 历史数据趋势
- 每 2 秒自动采样，最多保留 60 个数据点
- 折线图可视化（纯 UI 顶点绘制，不依赖 LineRenderer）
- 支持切换设备（A/B/C）与指标（温度/压力）
- 显示当前值、最高、最低与采样点数（标注数据来源：本地 / 服务器）
- 采样数据每 10s 批量上报至后端 `POST /api/history` 持久化到 SQLite
- 历史页在连接后端时，从 `GET /api/history/{设备}/{指标}?minutes=1440` 拉取长期数据绘制趋势；未连接时回退本地最近采样（带切换防抖）

### ⑧ 用户权限与操作日志

- **登录鉴权**：设置页新增登录区（用户名 / 密码 / 登录按钮）。点击登录调用 `POST /api/auth/login` 获取 Bearer token，后续所有请求由 `ApiClient` 自动附加 `Authorization` 头。Demo 账号：`admin/admin123`、`operator/operator123`
- **操作日志审计**：关键操作（登录、测试连接、手动故障模拟、设备恢复、设备上下线、报警产生、报警恢复）在连接后端且已登录时，自动上报 `POST /api/logs` 持久化到 `OperationLogs` 表，实现"谁在操作、做了什么、何时、对象是谁"的可追溯审计
- **权限保护**：写日志接口需登录（后端中间件校验 `Authorization: Bearer` 头，未登录返回 401），未登录的用户无法写入操作日志
- 操作日志可在后端 `GET /api/logs?limit=200` 查询（按时间倒序）

---

## 项目结构

```
Assets/
├── Editor/
│   ├── SetupIndustrialMonitorUI.cs    # 一键装配 UI 工具（含多车间+登录UI 一键装配）
│   ├── SetupWorkshops.cs              # 二号车间物理分区工具
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
│   │   ├── SettingsPanel.cs            # 设置页 + 连接测试 + 用户登录
│   │   ├── HistoryPanel.cs             # 历史记录页 + 趋势切换
│   │   ├── LineChart.cs                # 折线图组件（Graphic 子类）
│   │   └── TopBarController.cs         # 顶部状态栏联动
│   ├── Simulation/
│   │   ├── AlarmManager.cs             # 报警管理 + API 同步
│   │   └── HistoryRecorder.cs          # 历史数据采样器
│   ├── Data/
│   │   ├── AlarmRecord.cs              # 报警数据模型
│   │   └── ReportExporter.cs           # CSV 报表导出
│   └── Network/
│       └── ApiClient.cs                # HTTP 客户端单例（含登录鉴权 + 操作日志上报）
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
| `一键装配：多车间与登录UI` | 一键完成：复制二号车间物理分区 + 绑定概览车间统计文本 + 历史页车间切换按钮 + 设置页登录 UI（需先运行 V1 Setup） |

> 使用前提：需在非 Play 模式下运行（脚本已做防呆检查）。

---

## 后端 API

后端服务位于本仓库 `IndustrialMonitorAPI/` 目录，技术栈 **ASP.NET Core 10 + EF Core + SQLite + Swagger**。

提供设备管理、报警记录的 REST API 及数据持久化，详见 [IndustrialMonitorAPI/README.md](IndustrialMonitorAPI/README.md)。

### 后端分层设计

```
Controllers  →  处理 HTTP 请求，返回统一 ApiResponse<T>
Services     →  业务逻辑、参数校验、日志记录
DTOs         →  请求模型，隔离数据库实体
Common       →  统一响应模型
Data         →  EF Core 上下文与种子数据
```

**统一响应格式**（所有接口一致，含错误响应）：

```json
{ "code": 200, "message": "查询成功", "data": [] }
```

启动后访问 `http://localhost:5000/swagger` 可查看完整接口文档。

---

## 开发计划

- [x] 三维车间与设备建模
- [x] 设备选中与详情面板
- [x] 故障模拟与状态联动
- [x] 报警记录闭环（未恢复 / 已恢复）
- [x] 系统概览实时统计（含离线）
- [x] ASP.NET Core 后端 + SQLite
- [x] Unity ↔ API 联调
- [x] 历史数据折线图（温度 / 压力趋势）
- [x] 设备离线检测
- [x] 数据报表导出（CSV）
- [x] 顶部状态栏实时联动
- [x] 设备历史数据持久化到后端（EF Core 迁移 + HistoryController）
- [x] 报警阈值自动化（超阈值自动报警 + 自动恢复 + 自然仿真）
- [x] 多车间扩展（设备归属车间 + 概览分组统计 + 历史页车间筛选 + 后端按车间查询 + 二号车间物理分区）
- [x] 用户权限与操作日志（登录鉴权 + 操作日志审计：/api/auth/login、/api/logs）
