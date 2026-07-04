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

`CodexMonitor` 是一个 C#/.NET Windows 托盘应用, 用于读取 Codex OAuth 凭据并请求 ChatGPT 官方额度接口, 然后通过本地 HTTP 服务向 LiteMonitor 和 TrafficMonitor 插件提供 Codex 额度显示数据. 当 OAuth 凭据不存在或无效时, 额度显示为不可用, 不再回退读取本地 session 数据.

## 目录结构

- `CodexMonitor.Core`: 官方额度采集, 使用量缓存, HTTP 服务, 设置存储, LiteMonitor 和 TrafficMonitor 定位, 插件安装, Windows 自启动管理.
- `CodexMonitor.App`: WinForms 托盘应用和设置窗口.
- `CodexMonitor.Tests`: 自包含 C# 测试运行器.
- `Plugins/LiteMonitor`: LiteMonitor 插件定义, 当前插件文件为 `CodexMonitor.json`.
- `Plugins/TrafficMonitor`: TrafficMonitor 插件源码和配置模板.
- `Scripts`: 发布, 重启, release 打包, TrafficMonitor 插件构建脚本, 以及发布脚本共享逻辑.
- `Builds`: 发布产物目录, 只提交 `.gitkeep`, 其余内容由 `.gitignore` 忽略.
- `Directory.Build.props` 和 `Directory.Build.targets`: 全局 MSBuild 默认配置和默认编译项排除规则.

## 构建与输出

- `bin` 和 `obj` 使用各项目默认位置, 即 `{ProjectDir}/bin` 和 `{ProjectDir}/obj`, 由 `.gitignore` 的 `bin/` 和 `obj/` 规则过滤.
- 不把 `bin`, `obj` 重定向到 `Builds/` 下, 避免 Rider/ReSharper 找不到 `obj` 里的隐式生成文件而误报.
- `Builds/Output/` 存放本地发布和重启预览输出.
- `Builds/Release/vX.Y.Z/` 存放正式版本目录和对应 zip 包.
- `Configuration` 为空时默认按 `Debug` 处理.
- 不要把 `bin`, `obj`, `Builds` 下的任何生成文件提交到仓库.
- `Directory.Build.targets` 排除 `Builds/**` 下的 `.cs`, 防止发布产物里的 generated `.cs` 被 SDK 默认编译项重新纳入编译.

## 开发规则

- 修改 C# 代码时, namespace 必须与项目文件夹名对齐, 即 `CodexMonitor.Core`, `CodexMonitor.App`, `CodexMonitor.Tests`.
- LiteMonitor 插件模板文件名应保持为 `Plugins/LiteMonitor/CodexMonitor.json`.
- TrafficMonitor 插件模板文件名应保持为 `Plugins/TrafficMonitor/CodexMonitor.ini`.
- 插件模板作为发布内容复制到输出目录. 安装器不再内置兜底模板, 如果模板缺失应直接报错.
- 构造本地桥接 URL 时优先使用 `CodexMonitorDefaults.Host`, `CodexMonitorDefaults.DefaultBridgeUrl`, `CodexMonitorDefaults.DefaultBridgeTextUrl`, `CodexMonitorDefaults.BuildBridgeUrl`, 和 `CodexMonitorDefaults.BuildBridgeTextUrl`, 不要散落硬编码 `127.0.0.1` 或默认路径.
- LiteMonitor 使用 JSON 接口 `CodexMonitorDefaults.UsageEndpointPath`, 当前为 `/codex-monitor`.
- TrafficMonitor 原生插件使用文本接口 `CodexMonitorDefaults.UsageTextEndpointPath`, 当前为 `/codex-monitor.txt`, 两行依次为 5 小时额度和 Weekly 额度.
- LiteMonitor 和 TrafficMonitor 的自动定位共享 `CodexMonitor.Core/MonitorLocator.cs`, 不要重新复制磁盘搜索逻辑.
- `Scripts/Publish-App.ps1`, `Scripts/Restart-App.ps1`, 和 `Scripts/Package-Release.ps1` 共享 `Scripts/Publish-Shared.ps1`, 修改发布参数或清理逻辑时优先改共享脚本.

## 运行行为

- 应用启动后常驻 Windows 托盘. Windows 不提供受支持 API 让应用首次启动时强制 pin 到托盘可见区, 不要通过注册表或系统内部数据 hack 托盘 pin 状态.
- 当 `settings.json` 不存在时视为首次启动. 首次启动保存默认设置后自动打开主面板, 不跳转设置页.
- 通过 mutex 保证单实例. 用户再次启动 `CodexMonitor.exe` 时, 新进程只通过 `showPanelEvent` 通知现有进程打开主面板, 然后退出.
- 设置页已经合入托盘弹窗面板. 旧的 `showSettingsEvent` 命名已废弃, 后续不要恢复这个命名.

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

如果需要从资源管理器双击运行, 使用 `Scripts/Publish-App.cmd`, `Scripts/Restart-App.cmd`, 或 `Scripts/Build-TrafficMonitorPlugin.cmd`, 结束后窗口会停留显示结果.

打包 GitHub Release 上传文件:

```powershell
.\Scripts\Package-Release.ps1 -Version 0.1.0
```

输出文件名格式为 `Builds/Release/vX.Y.Z/CodexMonitor-vX.Y.Z-win-x64.zip`.

## 发布流程

- 当用户要求打 tag 或发布新版本并提供版本号时, 按本节自动完成后续流程.
- 版本号支持 `X.Y.Z` 或 `vX.Y.Z`, tag 固定使用 `vX.Y.Z`, 上传文件固定使用 `Builds/Release/vX.Y.Z/CodexMonitor-vX.Y.Z-win-x64.zip`.
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
- 修改插件输出字段或 HTTP 路径时, 同步检查 README 的 HTTP API 示例, LiteMonitor 插件提取路径, 和 TrafficMonitor 配置模板.
- Windows 沙箱环境可能阻止 `dotnet` 写入或删除 `Builds` 下的生成文件. 遇到这种情况时, 使用受控提权重新执行验证或清理命令.
