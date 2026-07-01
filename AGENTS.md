# CodexMonitor Agent Guide

## 目录

- [项目概览](#项目概览)
- [语言与文风](#语言与文风)
- [目录结构](#目录结构)
- [构建与输出](#构建与输出)
- [开发规则](#开发规则)
- [验证命令](#验证命令)
- [注意事项](#注意事项)

## 项目概览

`CodexMonitor` 是一个 C#/.NET Windows 托盘应用, 用于读取 Codex Desktop 写入的 `~/.codex/sessions/**/*.jsonl` 中的 `token_count` 事件, 并通过本地 HTTP 服务向 LiteMonitor 插件提供 Codex 额度显示数据.

当前工程只保留 C# 实现, 不包含 Python bridge 或 legacy PowerShell 启动脚本.

## 语言与文风

- 常规对话和 Markdown 文档使用中文文字和英文标点.
- C# 代码, 代码注释, XML summary, 项目文件, JSON 字段, 命令和配置使用英文.
- 文档中的路径, 命令, 类型名, 字段名和文件名使用反引号.
- 不要在正式代码, 配置, commit message 或项目文档正文中加入角色口癖或颜文字.

## 目录结构

- `CodexMonitor.Core`: 额度采集, HTTP 服务, 设置存储, LiteMonitor 定位, 插件安装, Windows 自启动管理.
- `CodexMonitor.App`: WinForms 托盘应用和设置窗口.
- `CodexMonitor.Tests`: 自包含 C# 测试运行器.
- `LiteMonitorPlugin`: LiteMonitor 插件定义, 当前插件文件为 `CodexMonitor.json`.
- `Builds`: 构建与发布产物目录, 只提交 `.gitkeep`, 其余内容由 `.gitignore` 忽略.
- `Directory.Build.props` 和 `Directory.Build.targets`: 全局 MSBuild 输出路径和默认编译项排除规则.

## 构建与输出

- 统一构建输出根目录为 `Builds/{Configuration}/{ProjectName}`.
- `bin` 输出路径为 `Builds/{Configuration}/{ProjectName}/bin`.
- `obj` 输出根路径为 `Builds/{Configuration}/{ProjectName}/obj`.
- `Configuration` 为空时默认按 `Debug` 处理.
- 不要把 `Builds/Debug`, `Builds/Release`, 或任何生成文件提交到仓库.
- 如果调整构建输出路径, 必须确认 generated `.cs` 文件不会被 SDK 默认编译项重新纳入编译.

## 开发规则

- 修改 C# 代码时, namespace 必须与项目文件夹名对齐, 即 `CodexMonitor.Core`, `CodexMonitor.App`, `CodexMonitor.Tests`.
- 新增或修改 C# 方法时, 方法声明上方必须有一句简洁英文 XML summary.
- 私有字段使用 `m_` 前缀, 私有常量使用 `k_` 前缀, 私有静态字段使用 `s_` 前缀.
- 保持现有文件的成员顺序, 缩进, 命名和局部抽象风格.
- 不要重新引入 Python bridge, legacy PowerShell 启动脚本, 或旧 `CodexUsage*` 命名.
- LiteMonitor 插件文件名应保持为 `LiteMonitorPlugin/CodexMonitor.json`.
- 如果修改内置插件 JSON, 同步检查 `CodexMonitor.Core/LiteMonitorPluginInstaller.cs` 中的 `PluginJson`.

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
dotnet publish .\CodexMonitor.App\CodexMonitor.App.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o .\Builds\Release\CodexMonitor.App\publish\win-x64
```

## 注意事项

- 默认 HTTP 服务只监听 `127.0.0.1`, 不要无意改成局域网可访问地址.
- 项目不读取 `~/.codex/auth.json`, 不访问 OpenAI API, 不接触 access token.
- 修改插件输出字段时, 同步检查 README 的 HTTP API 示例和 LiteMonitor 插件提取路径.
- Windows 沙箱环境可能阻止 `dotnet` 写入或删除 `Builds` 下的生成文件. 遇到这种情况时, 使用受控提权重新执行验证或清理命令.
