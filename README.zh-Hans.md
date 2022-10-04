# ObsidianShell
语言：[English](README.md)、[简体中文](README.zh-Hans.md)

## 功能
- 关联 Markdown 文件到 Obsidian  
  除了方便打开笔记外，关联文件还有助于将 Obsidian 集成进你的工作流，比如你可以使用一个启动器来快速打开笔记文件。

  配合支持拼音搜索的启动器，还可以间接实现通过拼音搜索笔记文件，比如：
  - [Everything](https://www.voidtools.com/) + [IbEverythingExt](https://github.com/Chaoses-Ib/IbEverythingExt)
  - [Listary](https://www.listarypro.com/)
- VaultRecent/Recent 模式
  - 在 Obsidian 中打开独立 Markdown 文件，将 Obsidian 用作一个 Markdown 编辑器
  - 实现全局仓库模式，让不同位置的笔记可以使用同一份配置
- 通过命令行在 Obsidian 中打开 Markdown 文件

## 安装
[Releases](https://github.com/Chaoses-Ib/ObsidianShell/releases)

配置文件路径：`C:\Program Files\Chaoses Ib\ObsidianShell\ObsidianCLI.exe.Config`
### 设为 Markdown 文件的默认程序
![](images/File%20list.png)

1. 右键单击一个 .md 文件
2. 选择 `打开方式` → `选择其他应用` → `ObsidianCLI`
3. 勾选 `始终使用此应用打开 .md 文件`
4. 点击 `确定`

## CLI
一个用来在 Obsidian 中打开 Markdown 文件的命令行程序。

支持三种打开模式：
- VaultFallback（默认）  
  如果 Markdown 文件在仓库中，打开仓库，否则就用 Markdown 回落（见下文）打开它。
- VaultRecent  
  如果 Markdown 文件在仓库中，打开仓库，否则把它链接到 Recent 仓库并打开。

  实现方法来自 [@etienne](https://forum.obsidian.md/t/open-and-edit-standalone-markdown-files/14977)。
- Recent  
  把 Markdown 文件链接到 Recent 仓库并打开。

打开模式可以在 `ObsidianCLI.exe.Config` 中设置：
```xml
<add key="OpenMode" value="VaultFallback" />
```

注意：要打开的 Markdown 文件的仓库必须在仓库列表中，也就是说，你必须在之前打开过那个仓库，否则 Obsidian 会报错。

### Markdown 回落
Markdown 回落编辑器可以在 `ObsidianCLI.exe.Config` 中设置。

记事本（默认）：
```xml
<add key="MarkdownFallback" value="notepad" />
<add key="MarkdownFallbackArguments" value="{0}" />
```

[Visual Studio Code](https://code.visualstudio.com/)：
```xml
<add key="MarkdownFallback" value="%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe" />
<add key="MarkdownFallbackArguments" value="{0}" />
```
（不推荐用 `code`，它实际上是个批处理文件，会导致一些延迟）

[Typora](https://typora.io/)（≥ 1.1）：
```xml
<add key="MarkdownFallback" value="C:\Program Files\Typora\Typora.exe" />
<add key="MarkdownFallbackArguments" value="{0}" />
```

### Recent 仓库
你可以在 `ObsidianCLI.exe.Config` 中设置 Recent 仓库的位置：
```xml
<add key="RecentVault" value="C:\path\to\Recent" />
```
以及 Recent 子目录的最大数量：
```xml
<add key="RecentLimit" value="10" />
```

注意：你必须在之前打开过 Recent 仓库，否则 Obsidian 会在打开文件时报错。

## ContextMenu
上下文菜单：  
![](images/ContextMenu.png)

用于在 Obsidian 中打开相应目录，补足 CLI 无法关联目录的缺陷。