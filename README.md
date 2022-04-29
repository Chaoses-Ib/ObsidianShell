# ObsidianShell
Languages: [English](README.md), [简体中文](README.zh-Hans.md)

## Features
- Open .md files in Obsidian through command line
- Associate .md files with Obsidian  
    This enables you to integrate Obsidian into your workflow. For example, you can use your favorite launcher to open your notes.
    
    For Chinese users:  
    配合支持拼音搜索的启动器，还可以间接实现通过拼音搜索笔记文件，比如：
    - [Everything](https://www.voidtools.com/) + [IbEverythingExt](https://github.com/Chaoses-Ib/IbEverythingExt)
    - [Listary](https://www.listarypro.com/)

## CLI
A command line interface program for opening .md files in Obsidian. If the .md file is not in a Vault, the program will instead open it using the Markdown fallback.

Notice: The Vault where the .md file in must be in the Vault list before opening the file, i.e., you must have opened that Vault before, otherwise Obsidian will report an error.

### Markdown fallback
You can change the Markdown fallback in `ObsidianCLI.exe.Config`.

Notepad (default):
```xml
<add key="MarkdownFallback" value="notepad" />
<add key="MarkdownFallbackArguments" value="{0}" />
```

[Visual Studio Code](https://code.visualstudio.com/):
```xml
<add key="MarkdownFallback" value="%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe" />
<add key="MarkdownFallbackArguments" value="{0}" />
```
(The use of `code` is not recommended since it is actually an batch file and leads to a delay)

[Typora](https://typora.io/) (≥ 1.1):
```xml
<add key="MarkdownFallback" value="C:\Program Files\Typora\Typora.exe" />
<add key="MarkdownFallbackArguments" value="{0}" />
```

### Set as the default program for .md files
![](images/File%20list.png)
1. Right-click on one .md file
2. Select `Open with` → `Choose another app` → `More Apps` → `Look for another app on this PC`
3. Find and choose `ObsidianCLI.exe`

## Download
[Releases](https://github.com/Chaoses-Ib/ObsidianShell/releases)

Requirements:
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (installed by default on Windows 10 1903 and later)