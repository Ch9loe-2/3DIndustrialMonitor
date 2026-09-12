# IndustrialMonitorAPI

三维工业设备监控与故障模拟系统的**后端服务**，为 Unity 前端提供设备数据、报警记录的持久化存储。

技术栈：ASP.NET Core 10 + Entity Framework Core + SQLite

---

## 快速启动

```bash
dotnet run --urls "http://localhost:5000"
```

启动后自动完成：
1. 创建 SQLite 数据库（`industrial_monitor.db`）
2. 建表（Devices / AlarmRecords）
3. 写入 3 台设备的种子数据（设备 A / B / C）

看到 `Now listening on: http://localhost:5000` 即启动成功。

---

## API 接口

### 设备

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/devices` | 获取全部设备 |
| GET | `/api/devices/{id}` | 获取单台设备 |
| PUT | `/api/devices/{id}` | 更新设备数据与状态 |

更新设备请求体示例：

```json
{
  "temperature": 95.0,
  "pressure": 1.62,
  "rpm": 1450,
  "runtime": 128.5,
  "status": "故障"
}
```

状态取值：`正常` / `警告` / `故障` / `离线`

### 报警记录

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/alarms` | 获取全部报警（按时间倒序） |
| GET | `/api/alarms/recent` | 获取最近一条未恢复报警 |
| POST | `/api/alarms` | 新增报警 |
| PUT | `/api/alarms/recover/{deviceName}` | 恢复该设备所有未恢复报警 |

新增报警请求体示例：

```json
{
  "deviceName": "设备 A",
  "alarmType": "温度过高",
  "alarmLevel": "故障",
  "alarmMessage": "温度达到 105.0 ℃"
}
```

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
| Initial* | float/int | 初始值（恢复时回退到这些值） |

### AlarmRecord（报警记录）

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | int | 主键 |
| DeviceName | string | 设备名 |
| AlarmType | string | 报警类型 |
| AlarmLevel | string | 报警等级 |
| AlarmMessage | string | 报警描述 |
| Time | DateTime | 发生时间 |
| Status | string | `未恢复` / `已恢复` |
| RecoverTime | DateTime? | 恢复时间 |

---

## 项目结构

```
IndustrialMonitorAPI/
├── Program.cs                     # 服务注册、CORS、自动建库
├── Models/
│   ├── Device.cs                  # 设备实体
│   └── AlarmRecord.cs             # 报警实体
├── Data/
│   └── AppDbContext.cs            # EF Core 上下文 + 种子数据
└── Controllers/
    ├── DevicesController.cs       # 设备接口
    └── AlarmsController.cs        # 报警接口
```

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

已开放 CORS 允许任意来源，方便 Unity Editor（localhost）直接调用。

---

## 联调说明

前端 Unity 项目通过 `ApiClient.cs` 调用本服务。启动顺序：

1. 先启动本 API（保持终端运行）
2. 再在 Unity 中点击 Play

Unity 设置页会显示连接状态，绿色 `● 已连接` 表示联通。

> 测试接口时注意：若系统设置了 HTTP 代理，`curl` 需加 `--noproxy '*'`，否则请求会被代理拦截返回 403。
