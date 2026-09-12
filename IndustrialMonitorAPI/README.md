# IndustrialMonitorAPI

三维工业设备监控与故障模拟系统的**后端服务**，为 Unity 前端提供设备数据、报警记录的持久化存储。

技术栈：ASP.NET Core 10 + Entity Framework Core + SQLite + Swagger

---

## 快速启动

```bash
dotnet run --urls "http://localhost:5000"
```

启动后通过 **EF Core 迁移（Migrate）** 自动完成：
1. 创建 SQLite 数据库（`industrial_monitor.db`）
2. 建表（Devices / AlarmRecords / DeviceMetricHistories / OperationLogs）
3. 写入 3 台设备的种子数据（设备 A / B / C）

> 数据库结构由 `Migrations/` 目录下的迁移脚本管理，首次启动自动应用，无需手动建表。

访问 **http://localhost:5000/swagger** 查看并测试接口文档。

---

## 统一响应格式

所有接口返回统一结构，便于前端统一处理：

```json
{
  "code": 200,
  "message": "查询成功",
  "data": [...]
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| code | int | 业务状态码 |
| message | string | 操作结果信息 |
| data | object | 返回的数据（可能为 null） |

### 错误响应示例

参数校验失败：

```json
{
  "code": 400,
  "message": "温度必须在 -50 ~ 500 ℃ 之间",
  "data": null
}
```

服务器异常（全局异常处理，不暴露内部信息）：

```json
{
  "code": 500,
  "message": "服务器内部发生错误",
  "data": null
}
```

---

## API 接口

### 设备

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/devices` | 获取全部设备 |
| GET | `/api/devices/{id}` | 根据 ID 获取设备 |
| PUT | `/api/devices/{id}` | 更新设备数据与状态 |

更新设备请求体（使用 `DeviceUpdateRequest` DTO）：

```json
{
  "temperature": 95.0,
  "pressure": 1.62,
  "rpm": 1450,
  "runtime": 128.5,
  "status": "故障"
}
```

校验规则：

| 字段 | 规则 |
|------|------|
| Temperature | -50 ~ 500 |
| Pressure | 0 ~ 100 |
| Rpm | 0 ~ 100000 |
| Runtime | 不能为负 |
| Status | 只能是 正常 / 警告 / 故障 / 离线 |

### 报警记录

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/alarms` | 获取全部报警（时间倒序） |
| GET | `/api/alarms/recent` | 获取最近一条未恢复报警 |
| POST | `/api/alarms` | 新增报警 |
| PUT | `/api/alarms/recover/{deviceName}` | 恢复该设备所有未恢复报警 |

新增报警请求体（使用 `AlarmCreateRequest` DTO）：

```json
{
  "deviceName": "设备 A",
  "alarmType": "温度过高",
  "alarmLevel": "故障",
  "alarmMessage": "温度达到 105.0 ℃"
}
```

---

### 历史数据（指标采样）

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/history` | 批量上报设备指标采样点 |
| GET | `/api/history/{deviceName}/{metricName}` | 查询某设备某指标最近 N 分钟历史（默认 30，范围 1~1440） |
| DELETE | `/api/history/cleanup` | 清理超过指定天数的历史数据（默认 7 天） |

批量上报请求体（使用 `MetricBatchRequest` DTO）：

```json
{
  "deviceName": "设备 A",
  "metricName": "温度",
  "points": [
    { "value": 66.5, "timestamp": "2026-09-12T03:00:00Z" },
    { "value": 67.1 }
  ]
}
```

> Unity 端的 `HistoryRecorder` 每 2 秒采样一次，可累积若干点后通过此接口批量上报，减少请求次数。

### 用户权限与操作日志

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/auth/login` | 用户登录，返回 Bearer token（Demo 账号：admin/admin123、operator/operator123） |
| GET | `/api/logs` | 查询最近操作日志（按时间倒序） |
| POST | `/api/logs` | 记录操作日志（**需登录**：未带 `Authorization: Bearer` 头返回 401） |

登录请求体：

```json
{ "username": "admin", "password": "admin123" }
```

操作日志请求体（使用 `OperationLogRequest` DTO）：

```json
{ "action": "故障模拟", "target": "设备 A", "detail": "手动触发温度过高故障" }
```

> 写日志接口由 `Program.cs` 中间件保护：仅允许携带有效 Bearer token 的请求，实现"未登录不能写入操作日志"的权限控制。操作记录持久化到 `OperationLogs` 表（操作人 / 类型 / 对象 / 详情 / 时间）。

---

## 数据模型

### Device（设备）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| DeviceName | string | 设备名 |
| DeviceType | string | 设备类型 |
| Temperature | float | 温度（℃） |
| Pressure | float | 压力（MPa） |
| Rpm | int | 转速 |
| Runtime | float | 运行时长（h） |
| Status | string | 状态 |
| Initial* | float/int | 初始值（恢复时回退） |

### AlarmRecord（报警记录）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| DeviceName | string | 设备名 |
| AlarmType | string | 报警类型 |
| AlarmLevel | string | 报警等级 |
| AlarmMessage | string | 报警描述 |
| Time | DateTime | 发生时间 |
| Status | string | 未恢复 / 已恢复 |
| RecoverTime | DateTime? | 恢复时间 |

### DeviceMetricHistory（指标历史）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| DeviceName | string | 设备名 |
| MetricName | string | 指标名（温度 / 压力 / 转速） |
| Value | float | 采样值 |
| Timestamp | DateTime | 采样时间（UTC） |

### OperationLog（操作日志）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| UserName | string | 操作人（来自登录 token；未登录记"匿名"） |
| Action | string | 操作类型（登录 / 故障模拟 / 恢复 / 设备上下线 / 报警产生 / 报警恢复 ...） |
| Target | string | 操作对象（设备名 / 接口） |
| Detail | string | 操作详情 |
| IpAddress | string | 来源 IP |
| Timestamp | DateTime | 操作时间（UTC） |

---

## 项目结构

```
IndustrialMonitorAPI/
├── Program.cs                      # 服务注册、全局异常处理、Swagger
├── Common/
│   └── ApiResponse.cs              # 统一 API 响应模型
├── Controllers/
│   ├── DevicesController.cs        # 设备接口
│   ├── AlarmsController.cs         # 报警接口
│   ├── HistoryController.cs         # 历史数据接口
│   ├── AuthController.cs          # 用户登录接口
│   └── OperationLogsController.cs  # 操作日志接口
├── DTOs/
│   ├── DeviceUpdateRequest.cs      # 设备更新请求模型
│   ├── AlarmCreateRequest.cs       # 报警创建请求模型
│   ├── MetricHistoryRequest.cs      # 历史数据上报请求模型
│   └── OperationLogRequest.cs      # 操作日志请求模型
├── Services/
│   ├── DeviceService.cs            # 设备业务逻辑
│   ├── AlarmService.cs             # 报警业务逻辑
│   ├── MetricHistoryService.cs       # 历史数据业务逻辑
│   ├── AuthService.cs              # 登录鉴权（Demo 账号，内存 token）
│   └── OperationLogService.cs       # 操作日志业务逻辑
├── Models/
│   ├── Device.cs                   # 设备实体
│   ├── AlarmRecord.cs              # 报警实体
│   ├── DeviceMetricHistory.cs      # 指标历史实体
│   └── OperationLog.cs            # 操作日志实体
└── Data/
    └── AppDbContext.cs             # EF Core 上下文 + 种子数据
└── Migrations/                      # EF Core 迁移脚本
```

---

## 分层设计

### Controllers

负责 HTTP 请求与响应处理，调用 Service 完成业务，返回统一的 `ApiResponse<T>`。不直接操作数据库。

### Services

负责业务逻辑，包括数据校验、数据库操作与日志记录。两个 Service 均通过依赖注入获取 `AppDbContext` 与 `ILogger`。

### DTOs

用于接收客户端请求数据，避免直接使用数据库实体作为请求模型（防止客户端篡改主键等越权风险）。参数校验使用 DataAnnotations 特性。

### Common

存放通用数据结构，如统一响应模型 `ApiResponse<T>`。

### Data

EF Core 数据库上下文配置与种子数据。

---

## 设计要点

- **统一响应格式**：所有接口（含校验失败、服务器异常）均返回 `{code, message, data}`
- **参数校验**：DTO 上使用 DataAnnotations，校验失败由 `InvalidModelStateResponseFactory` 统一转为 ApiResponse
- **全局异常处理**：`UseExceptionHandler` 捕获未处理异常，返回 500 且不暴露堆栈信息
- **日志记录**：Service 层使用 `ILogger` 记录关键操作与警告
- **Swagger 文档**：每个接口带 `EndpointSummary` / `EndpointDescription` 说明
- **CORS**：允许任意来源，方便 Unity Editor（localhost）调用
- **登录鉴权**：`AuthController` 提供登录接口，`AuthService` 签发内存 token；前端 `ApiClient` 登录后自动附加 `Authorization` 头
- **操作日志审计**：关键操作自动上报 `POST /api/logs`；中间件保护写接口（未登录返回 401），`OperationLog` 持久化到 `OperationLogs` 表
- **数据库迁移**：使用 EF Core Migrations 管理表结构，启动时 `Migrate()` 自动应用；种子数据通过 `HasData` 固化（避免动态默认值导致模型不确定）

---

## 配置

数据库连接串在 `appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=industrial_monitor.db"
  }
}
```

---

## 联调说明

前端 Unity 项目通过 `ApiClient.cs` 调用本服务。启动顺序：

1. 先启动本 API（保持终端运行）
2. 再在 Unity 中点击 Play

Unity 设置页会显示连接状态，绿色 `● 已连接` 表示联通。

> Unity 端目前只判断 HTTP 成功与否，不解析响应内容，因此后端改为统一响应格式后前端无需改动。
>
> 测试接口时注意：若系统设置了 HTTP 代理，`curl` 需加 `--noproxy '*'`，否则请求会被代理拦截返回 403。
