# HIK_DEMON — 海康机器人相机视觉终端

基于 C# WinForms 的工业相机视觉终端，集成**相机控制、QR 码实时检测与 3D 位姿估计、多目标跟踪、手指关节角度测量、串口通信输出**等功能，适用于工业自动化、机器人引导、手部姿态监测等场景。

## 功能概览

### 相机控制
- 枚举海康 GigE / USB 工业相机，查看设备信息
- 打开/关闭相机，支持连续采集与单帧采集
- 实时显示相机画面，独立线程采集保证主界面流畅
- 参数读取与下发：曝光时间、增益、像素格式

### QR 码检测与 3D 位姿估计
- 使用 OpenCV 微信 QR 码识别模块（WeChatQRCode）进行深度学习二维码检测与解码
- 通过 `SolvePnP` + 相机内参解算二维码在相机坐标系下的 3D 空间坐标（X, Y, Z）
- 支持多二维码同时检测，画面实时框选并标注坐标
- CLAHE 自适应直方图均衡 + 锐化预处理，提升复杂光照下的识别率

### 多二维码跟踪与管理
- 自带二维码数据库，跨帧追踪同一二维码
- 支持为每个二维码手动分配 ID，在线/离线状态自动标识
- 所有二维码与当前可见二维码分别列表展示

### 手指关节角度测量
- 将多个二维码按 ID 顺序视作手指关节节点
- 自动计算相邻节点的 3D 空间距离
- 计算连续三点（如 ID 1-2-3）形成的关节夹角
- 通过串口将角度数据实时发送给外部设备

### 相机标定
- 棋盘格标定板（9×6 方格，边长 23.7mm，**参数可在 UI 中自由修改**）标定工具
- 自动提取角点、重投影误差评估、粗差剔除
- 输出相机内参矩阵和畸变系数
- **标定达标（RMS<1.0）自动保存至 config.json，立即更新相机内参**

### 串口通信
- 自动刷新可用 COM 口，支持常用波特率（9600~115200）
- 角度数据帧格式：1 字节 ID + 4 字节 float 角度 = 5 字节

### 配置文件
- 项目根目录下的 `config.json` 统一管理所有可配置参数
- 标定板尺寸、相机内参、QR 码参数、曝光/增益等均可保存与恢复
- 程序启动自动加载，标定达标自动写入，也可点击 **写入配置** 手动保存

## 界面布局

```
┌──────────────────────────────────────────────────────────┐
│  实时画面与控制  │  二维码与串口                          │  ← Tab 页
├────────────────────┬─────────────────────────────────────┤
│                    │  设备连接          │  QR码位姿       │
│                    │  [枚举] [打开]     │  ☑启用识别      │
│   相机实时画面     │  [关闭]            │  边长(mm):[___]  │
│                    ├────────────────────┤  ┌───────────┐ │
│                    │  采集控制          │  │ 位姿结果   │ │
│                    │  [单帧][连续][停止] │  │           │ │
│                    ├────────────────────┤  └───────────┘ │
│                    │  参数配置          ├─────────────────┤
│                    │  曝光时间:[___]    │  棋盘格标定      │
│                    │  增益:[___]        │  [采集] [执行]   │
│                    │  实际帧率:[___]    │  棋盘格:9×6      │
│                    │  像素格式:[_____]  │  方格边长:23.7mm │
│                    │  [获取参数] [下发]  │  [写入配置]      │
│                    │                    │  ┌───────────┐ │
├────────────────────┴─────────────────────────────────────┤
│  调试日志                                                  │
├──────────────────────────────────────────────────────────┤
│ ○ 相机未连接 │ ○ 未采集 │ QR: 关闭 │ ○ 串口关闭         │  ← 状态栏
└──────────────────────────────────────────────────────────┘
```

- **双列布局**：右侧面板分两列排列，减少滚动
- **状态栏**：底部显示相机、采集、QR、串口四个状态灯
- **ToolTip**：所有功能按钮悬停显示中文说明

## 技术栈

| 组件 | 技术 |
|------|------|
| 语言与框架 | C# + .NET Framework 4.7.2 + WinForms |
| 相机 SDK | 海康机器人 MVS SDK（MvCameraControl.Net） |
| 图像处理 | OpenCvSharp 4.13 |
| QR 码识别 | OpenCV WeChatQRCode（Caffe 模型） |
| 串口通信 | System.IO.Ports |

## 项目结构

```
HIK_DEMON/
├── Form1.cs                    # 主窗口逻辑
├── Form1.Designer.cs           # 界面布局
├── Form1.resx                  # 窗体资源
├── Program.cs                  # 入口点
├── WeChatQRCodeDecoder.cs      # QR 码识别引擎封装
├── ProjectConfig.cs            # 配置文件数据模型
├── ConfigManager.cs            # config.json 读写
├── model/                      # WeChatQRCode 深度学习模型
│   ├── detect.prototxt
│   ├── detect.caffemodel
│   ├── sr.prototxt
│   └── sr.caffemodel
├── HIK_DEMON.csproj            # 项目文件
├── HIK_DEMON.sln               # 解决方案文件
├── packages.config             # NuGet 包引用
├── .editorconfig               # 代码风格配置
├── .gitignore
├── .vscode/
│   ├── tasks.json              # 编译任务（Ctrl+Shift+B）
│   ├── launch.json             # 调试配置（F5）
│   ├── settings.json           # 编辑器设置
│   └── extensions.json         # 推荐扩展
└── README.md
```

## VSCode 开发环境

### 前置要求

| 依赖 | 说明 |
|------|------|
| [VSCode](https://code.visualstudio.com/) | 代码编辑器 |
| [.NET SDK 8.0+](https://dotnet.microsoft.com/download) | 内置 MSBuild，可编译 .NET Framework 项目 |
| 海康 MVS SDK | `MvCameraControl.Net.dll` 位于 `E:\MVS\Development\` |

### 安装扩展

`.vscode/extensions.json` 已配置推荐扩展，打开项目时会自动提示安装：

| 扩展 | ID | 用途 |
|------|----|------|
| **C#** | `ms-dotnettools.csharp` | OmniSharp 语言服务（**本项目的核心**） |
| **C# Dev Kit** | `ms-dotnettools.csdevkit` | 可选，增强型工具，但本项目以 C# 扩展为主 |
| **IntelliCode for C#** | `ms-dotnettools.vscodeintellicode-csharp` | AI 辅助代码补全 |

> ⚠ **重要**：本项目的调试器使用 `"type": "clr"`（即 OmniSharp CLR 调试器）。如果安装了 C# Dev Kit，请在 `.vscode/settings.json` 中确认 `"dotnet.preferCSharpExtension": true` 保持启用，否则调试可能不工作。

### 常用操作

| 操作 | 方式 |
|------|------|
| 编译 | `Ctrl+Shift+B`（默认 build 任务） |
| 调试启动 | `F5` → "Launch HIK_DEMON (Debug)" |
| 运行（不调试） | `Ctrl+F5` → "仅编译 (不启动)" |
| Release 编译 | `Ctrl+Shift+B` → 选 "build-release" |
| 清理 | 终端执行 `dotnet clean -p:Platform=x64 -p:Configuration=Debug` |

### 终端命令

```bash
# 编译（x64 Debug）
dotnet build -p:Platform=x64 -p:Configuration=Debug

# 编译（x64 Release）
dotnet build -p:Platform=x64 -p:Configuration=Release

# 运行
.\bin\x64\Debug\HIK_DEMON.exe
```

> ⚠ 项目必须用 **x64** 平台编译，OpenCV 和 MVS SDK 均为 64 位。

## 使用流程

1. 点击 **枚举** 搜索相机 → 选中相机 → **打开相机**
2. 点击 **连续采集** 启动实时画面
3. 勾选 **启用识别** 开始 QR 码检测与位姿估计
4. 在 **二维码与串口** Tab 页为二维码分配 ID
5. 打开串口，角度数据将自动发送到外部设备
6. 如需标定相机，使用棋盘格标定功能采集并计算内参

所有功能按钮均配有中文 ToolTip，鼠标悬停即可查看用途。

## 配置文件 (config.json)

程序启动时自动读取 `config.json`（位于运行目录），不存在则使用内置默认值。

配置项包含：

| 分组 | 字段 | 说明 |
|------|------|------|
| 标定板 | `BoardWidth`, `BoardHeight`, `SquareSize` | 棋盘格内角点数与方格边长(mm) |
| 相机内参 | `Fx`, `Fy`, `Cx`, `Cy` | 焦距和光心 |
| 畸变系数 | `K1`, `K2`, `P1`, `P2`, `K3` | 径向/切向畸变 |
| 标定质量 | `LastReprojectionError`, `LastCalibrated` | 上次标定的 RMS 与时间 |
| QR | `QRSize`, `QREnabled` | 二维码物理边长与开关 |
| 运行时参数 | `ExposureTime`, `Gain`, `PixelFormat` | 相机曝光/增益/像素格式 |

**保存时机**：
1. **自动保存** — 标定完成后 RMS<1.0 则自动写入内参并更新相机
2. **手动保存** — 点击标定面板上的 **写入配置** 按钮

**应用内修改参数**：
- 修改棋盘格宽/高/边长 → 直接标定即可使用新参数
- 修改曝光/增益 → 点击 **下发参数** 到相机，再按 **写入配置** 保存

## 常见问题

### ToolTip 构造函数异常

```
System.ArgumentNullException: 值不能为 null。
  在 System.Windows.Forms.ToolTip..ctor(IContainer cont)
```

**原因**：`ToolTip(IContainer)` 构造函数需要 `components` 容器已初始化。

**解决**：已在 `Form1.Designer.cs` 中添加 `this.components = new System.ComponentModel.Container();` 确保容器在使用前初始化。如果手动修改 Designer 文件，注意保持这行代码。

### 编译报错：进程锁定 exe

```
error MSB3027: 无法复制 exe，文件被 "HIK_DEMON (PID)" 锁定
```

**原因**：前一次运行的程序没有完全退出，进程仍在后台。

**解决**：

```bash
# 方式1：命令行终止
taskkill //F //IM HIK_DEMON.exe

# 方式2：任务管理器
# 在任务管理器中找到 HIK_DEMON.exe 进程，手动结束

# 方式3：重启 VSCode 终端
exit   # 关闭当前终端
# 重新打开终端
```

### OmniSharp 不工作 / 代码无智能提示

**原因**：OmniSharp 服务器未能正确启动。

**解决**：
1. 点击 VSCode 底部状态栏右侧的 OmniSharp 图标（⊗ 或 !）
2. 选择 "重启 OmniSharp"
3. 等待底部状态栏显示 "OmniSharp 就绪"

如果持续失败，尝试：
- 关闭 VSCode，删除项目下的 `.vs/` 目录，重新打开
- 检查 `.vscode/settings.json` 中 `"omnisharp.enableRoslynAnalyzers": true` 是否启用

### 调试器无法附加 / F5 无反应

**原因**：调试器类型与安装的扩展不匹配。

**解决**：
1. 确认安装了 **C#** 扩展（`ms-dotnettools.csharp`），这是支持 .NET Framework CLR 调试的必要扩展
2. 检查 `.vscode/settings.json` 中 `"dotnet.preferCSharpExtension": true` 已设置
3. 检查 `.vscode/launch.json` 中 `"type": "clr"`（仅 OmniSharp 支持）
4. 打开命令面板（`Ctrl+Shift+P`），运行 "OmniSharp: Select Project"，选择 `HIK_DEMON.csproj`

> 如果安装了 C# Dev Kit 但仍希望使用 CLR 调试器，可将 `.vscode/settings.json` 中的 `"dotnet.preferCSharpExtension"` 设为 `true`，并重启 VSCode。
