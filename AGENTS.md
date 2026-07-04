# CodexMonitor Agent Guide

## 目录

- [项目概览](#项目概览)
- [语言与文风](#语言与文风)
- [目录结构](#目录结构)
- [构建与输出](#构建与输出)
- [开发规则](#开发规则)
- [运行行为](#运行行为)
- [验证命令](#验证命令)
- [发布流程](#发布流程)
- [注意事项](#注意事项)

## 项目概览

`CodexMonitor` 是一个 C#/.NET 9 Windows 托盘应用, 用于读取 Codex OAuth 凭据并请求 ChatGPT 官方额度接口, 然后通过本地 HTTP 服务向 LiteMonitor 和 TrafficMonitor 插件提供 Codex 额度显示数据. 当 OAuth 凭据不存在或无效时, 额度显示为不可用, 不回退读取本地 session 数据.

当前桌面端是 WPF 托盘弹窗应用. `System.Windows.Forms` 仍用于 `NotifyIcon`, 文件夹选择, 系统消息框和 WinForms 应用初始化, 不代表 UI 主体仍是 WinForms.

## 语言与文风

- 常规项目文档使用中文文字和英文标点.
- 代码和代码注释使用英文.
- 面向用户的 UI 文案按现有英文风格维护, 不做无关中英文切换.

## 目录结构

- `CodexMonitor.Core`: 官方额度采集, 使用量缓存, HTTP 服务, 设置存储, LiteMonitor 和 TrafficMonitor 定位, 插件安装, Windows 自启动管理.
- `CodexMonitor.App`: WPF 托盘应用, 托盘控制器, Home/Settings 弹窗, ViewModel, 命令和自定义数值输入控件.
- `CodexMonitor.Tests`: 自包含 C# 测试运行器.
- `Plugins/LiteMonitor`: LiteMonitor 插件定义, 当前插件文件为 `CodexMonitor.json`.
- `Plugins/TrafficMonitor`: TrafficMonitor 原生插件源码和配置模板, 构建输出位于 `Plugins/TrafficMonitor/Builds/**`.
- `Resources`: 应用图标资源, 发布时复制到输出目录.
- `Docs`: README 展示图等文档资源.
- `Scripts`: 发布, 重启, release 打包, TrafficMonitor 插件构建脚本, 以及发布脚本共享逻辑.
- `Builds`: 发布产物目录, 只提交 `.gitkeep`, 其余内容由 `.gitignore` 忽略.
- `Directory.Build.props` 和 `Directory.Build.targets`: 全局 MSBuild 默认配置和默认编译项排除规则.

## 构建与输出

- `bin` 和 `obj` 使用各项目默认位置, 即 `{ProjectDir}/bin` 和 `{ProjectDir}/obj`, 由 `.gitignore` 的 `bin/` 和 `obj/` 规则过滤.
- 不把 `bin`, `obj` 重定向到 `Builds/` 下, 避免 Rider/ReSharper 找不到 `obj` 里的隐式生成文件而误报.
- `Builds/Output/win-x64/` 存放本地发布和重启预览输出.
- `Builds/Release/vX.Y.Z/` 存放正式版本目录和对应 zip 包. 带后缀版本使用同名 tag 目录, 例如 `Builds/Release/v1.2.3-beta.1/`.
- `Configuration` 为空时默认按 `Debug` 处理.
- 不要把 `bin`, `obj`, `Builds` 下的任何生成文件提交到仓库.
- `Directory.Build.targets` 排除 `Builds/**` 下的 `.cs`, 防止发布产物里的 generated `.cs` 被 SDK 默认编译项重新纳入编译.
- `CodexMonitor.App` 发布为 `net9.0-windows` 的 `win-x64` 单文件框架依赖应用, 外部复制 `Resources` 和 `Plugins` 模板目录.
- TrafficMonitor 原生插件 DLL 只有在已构建并存在 `Plugins/TrafficMonitor/Builds/x64/Release/CodexMonitor.dll` 时才随 App 发布复制.

## 开发规则

- 修改 C# 代码时, namespace 必须与项目文件夹名对齐, 即 `CodexMonitor.Core`, `CodexMonitor.App`, `CodexMonitor.Tests`.
- WPF UI 入口保持在 `CodexMonitor.App/App.cs`, `TrayController.cs`, `TrayPopupWindow.xaml`, `TrayPopupWindow.xaml.cs`, 和 `TrayPopupViewModel.cs`.
- 托盘图标继续使用 `System.Windows.Forms.NotifyIcon`; 不要为了纯 WPF 化重写托盘层.
- 不要恢复独立 WinForms 设置窗口. 设置页已经合入 WPF 托盘弹窗, 通过 Home/Settings 页切换.
- WPF 主题使用现有 Fluent 资源和 `ThemeMode` 设置, 支持 `System`, `Light`, `Dark`.
- 数值设置使用现有 `NumericUpDown` 和 `NumericInput`, 不新增第三方控件库.
- LiteMonitor 插件模板文件名应保持为 `Plugins/LiteMonitor/CodexMonitor.json`.
- TrafficMonitor 插件模板文件名应保持为 `Plugins/TrafficMonitor/CodexMonitor.ini`.
- 插件模板作为发布内容复制到输出目录. 安装器不内置兜底模板, 如果模板缺失应直接报错.
- 端口, 主机名, HTTP 路径, 文件名, 目录名, 默认设置值, 以及其他会被多处使用的参数, 优先复用或新增到 `CodexMonitorDefaults`, 不要在调用点散落硬编码常量.
- LiteMonitor 使用 JSON 接口 `CodexMonitorDefaults.UsageEndpointPath`, 当前为 `/codex-monitor`.
- TrafficMonitor 原生插件使用文本接口 `CodexMonitorDefaults.UsageTextEndpointPath`, 当前为 `/codex-monitor.txt`, 两行依次为 5 小时额度和 Weekly 额度.
- 健康检查接口为 `CodexMonitorDefaults.HealthEndpointPath`, 当前为 `/health`.
- LiteMonitor 和 TrafficMonitor 的自动定位通过 `LiteMonitorLocator`, `TrafficMonitorLocator`, 和共享 `MonitorLocator` 实现, 不要复制磁盘搜索逻辑.
- `Scripts/Publish-App.ps1`, `Scripts/Restart-App.ps1`, 和 `Scripts/Package-Release.ps1` 共享 `Scripts/Publish-Shared.ps1`, 修改发布参数或清理逻辑时优先改共享脚本.

## 运行行为

- 应用启动后常驻 Windows 托盘. Windows 不提供受支持 API 让应用首次启动时强制 pin 到托盘可见区, 不要通过注册表或系统内部数据 hack 托盘 pin 状态.
- 左键点击托盘图标切换 WPF 弹窗显示. 右键菜单只包含 `Open Panel`, `Refresh Now`, 和 `Exit`.
- WPF 弹窗包含 Home 和 Settings 两页. Home 展示计划类型, 更新时间, 5 小时额度和 Weekly 额度. Settings 管理刷新间隔, 主题, 开机自启, LiteMonitor 路径, TrafficMonitor 路径和 HTTP 端口.
- 当 `settings.json` 不存在时视为首次启动. 首次启动保存默认设置后自动打开主面板.
- `settings.json` 存放在 `CodexMonitor.exe` 同级目录, 加载缺失字段时会补齐并写回默认值.
- 默认刷新间隔为 `CodexMonitorDefaults.RefreshIntervalMinutes`, 当前为 1 分钟. 设置允许范围为 1 到 1440 分钟.
- 通过 mutex 保证单实例. 用户再次启动 `CodexMonitor.exe` 时, 新进程只通过 `showPanelEvent` 通知现有进程打开主面板, 然后退出.
- `showSettingsEvent` 命名已废弃, 后续不要恢复这个命名.

## 验证命令

构建全部项目:

```powershell
dotnet build .\CodexMonitor.sln -m:1
```

运行测试:

```powershell
dotnet run --project .\CodexMonitor.Tests\CodexMonitor.Tests.csproj
```

发布托盘应用:

```powershell
.\Scripts\Publish-App.ps1
```

构建 TrafficMonitor 原生插件:

```powershell
.\Scripts\Build-TrafficMonitorPlugin.ps1
```

修改托盘应用后, 验证通过时需要发布并重启预览程序:

```powershell
.\Scripts\Restart-App.ps1
```

如果需要从资源管理器双击运行, 使用 `Scripts/Publish-App.cmd`, `Scripts/Restart-App.cmd`, `Scripts/Package-Release.cmd`, 或 `Scripts/Build-TrafficMonitorPlugin.cmd`, 结束后窗口会停留显示结果.

打包 GitHub Release 上传文件:

```powershell
.\Scripts\Package-Release.ps1 -Version 0.1.0
```

输出文件名格式为 `Builds/Release/vX.Y.Z/CodexMonitor-vX.Y.Z-win-x64.zip`.

## 发布流程

- 当用户要求打 tag 或发布新版本并提供版本号时, 按本节自动完成后续流程.
- 版本号支持 `X.Y.Z`, `vX.Y.Z`, 或 SemVer 后缀形式, tag 固定使用 `v<normalized-version>`.
- 上传文件固定使用 `Builds/Release/v<normalized-version>/CodexMonitor-v<normalized-version>-win-x64.zip`.
- 发布前先检查 `git status --short --branch`, 确认本次待提交内容只包含发布相关修改.
- 如有发布脚本, 文档, 图标资源, 或应用打包行为变更, 先提交并推送这些源码修改. 不要提交 `Builds/Debug`, `Builds/Release`, 或 zip 产物.
- 执行 `.\Scripts\Package-Release.ps1 -Version X.Y.Z -NoPause` 生成 zip, 并确认 zip 文件存在.
- 在已推送的最终发布提交上执行 `git tag -a vX.Y.Z -m "vX.Y.Z"`, 再执行 `git push origin vX.Y.Z`.
- 使用 GitHub CLI 创建 release 并上传 zip:

```powershell
gh release create vX.Y.Z `
  "Builds\Release\vX.Y.Z\CodexMonitor-vX.Y.Z-win-x64.zip" `
  --title "CodexMonitor vX.Y.Z" `
  --notes "Release vX.Y.Z." `
  --verify-tag
```

- 如果对应 tag 或 release 已存在, 先停止并说明当前状态, 不要覆盖 tag. 替换 release asset 需要用户明确同意后才使用 `gh release upload --clobber`.
- 如果 `git push` 被 non-fast-forward 拒绝, 停止流程并交给用户决定是否 rebase 或 merge.

## 注意事项

- 默认 HTTP 服务只监听 `127.0.0.1`, 不要无意改成局域网可访问地址.
- 项目会读取 `~/.codex/auth.json` 中的 Codex OAuth token, 仅用于请求 `https://chatgpt.com/backend-api/wham/usage`. 不要在日志, 本地 HTTP 响应, README 示例, 或插件 JSON 中暴露 access token.
- 修改插件输出字段或 HTTP 路径时, 同步检查 README 的 HTTP API 示例, LiteMonitor 插件提取路径, TrafficMonitor 配置模板, TrafficMonitor 原生插件源码, 以及 C# 测试.
- 修改 WPF 弹窗布局或主题时, 同步检查 `Docs/showcase.png` 是否需要更新.
- Windows 沙箱环境可能阻止 `dotnet` 写入或删除 `Builds` 下的生成文件. 遇到这种情况时, 使用受控提权重新执行验证或清理命令.
