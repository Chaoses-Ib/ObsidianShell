## Release
1. Bump version
   - `ObsidianShell.CLI/Properties/AssemblyInfo.cs`
   - `ObsidianShell.ContextMenu/Properties/AssemblyInfo.cs`
   - `ObsidianShell.GUI/Properties/AssemblyInfo.cs`
2. Build the solution
3. Copy files from `bin\Release` to `Installer\bin` (without PDB and XML files) and make a `Settings.json`
4. Build the installer by Advanced Installer
5. Pack files in `Installer\bin`  as the protable version (in a directory named `ObsidianShell`)