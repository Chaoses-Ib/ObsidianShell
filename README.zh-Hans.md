# ObsidianShell
语言：[English](README.md)、[简体中文](README.zh-Hans.md)

## 功能
- 通过命令行在 Obsidian 中打开 .md 文件
- 关联 .md 文件到 Obsidian  
    这有助于将 Obsidian 集成进你的工作流。比如，你可以使用你喜欢的启动器来打开笔记文件。

    配合支持拼音搜索的启动器，还可以间接实现通过拼音搜索笔记文件，比如：
    - [Everything](https://www.voidtools.com/) + [IbEverythingExt](https://github.com/Chaoses-Ib/IbEverythingExt)
    - [Listary](https://www.listarypro.com/)

## CLI
一个用来在 Obsidian 中打开 .md 文件的命令行程序。如果 .md 文件不在库里，程序会改用 Markdown 回落来打开它。

注意：要打开的 .md 文件的库必须在库列表中，也就是说，你必须在之前打开过那个库，否则 Obsidian 会报错。

### Markdown 回落
Markdown 回落可以在 `ObsidianCLI.exe.Config` 里设置。

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

### 设为 .md 文件的默认程序
![](images/File%20list.png)
1. 右键单击一个 .md 文件
2. 选择 `打开方式` → `选择其他应用` → `更多应用` → `在这台电脑上查找其他应用`
3. 找到并选择 `ObsidianCLI.exe`

## 下载
[Releases](https://github.com/Chaoses-Ib/ObsidianShell/releases)

要求：
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)（在 Windows 10 1903 及以上默认已安装）