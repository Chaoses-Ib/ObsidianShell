## Release
1. Bump version
   - `CLI/Properties/AssemblyInfo.cs`
   - `ContextMenu/Properties/AssemblyInfo.cs`
2. Build the solution
3. Copy files from `CLI\bin\Release` and `ContextMenu\bin\Release` to `Installer\bin` (without PDB and XML files)
4. Build the installer by Advanced Installer
5. Pack files in `Installer\bin` as the protable version (in a directory named `ObsidianShell`)