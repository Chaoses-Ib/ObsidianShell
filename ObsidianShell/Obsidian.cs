using NCode.ReparsePoints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ObsidianShell
{
    public class Obsidian
    {
        private Settings _settings;

        public Obsidian(Settings settings)
        {
            _settings = settings;
        }
        
        public async Task OpenFile(string path)
        {
            switch (_settings.OpenMode)
            {
                case OpenMode.VaultFallback:
                    {
                        if (IsFileInVault(path))
                        {
                            await OpenFileInVault(path);
                        }
                        else
                        {
                            OpenFileByFallback(path);
                        }
                        break;
                    }
                case OpenMode.VaultRecent:
                    {
                        if (IsFileInVault(path))
                        {
                            await OpenFileInVault(path);
                        }
                        else
                        {
                            await OpenFileInRecent(path);
                        }
                        break;
                    }
                case OpenMode.Recent:
                    {
                        await OpenFileInRecent(path);
                        break;
                    }
            }
        }

        private static string PercentEncode(string text)
        {
            // WebUtility.UrlEncode() replaces space with "+" instead of "%20"
            // Uri.EscapeUriString() replaces space with "%20" but does not replace most reserved characters (!#$&'()+,;=@[])
            return Uri.EscapeDataString(text);
        }

        private async Task OpenFileInVault(string path, string vaultPath = null)
        {
            if (_settings.EnableAdvancedURI is false)
            {
                Process.Start($"obsidian://open?path={PercentEncode(path)}");
            }
            else
            {
                vaultPath = vaultPath ?? GetFileVaultPath(path);
                string vault = GetVaultName(vaultPath);
                string filepath = Utils.GetRelativePath(vaultPath, path);

                ObsidianOpenMode obsidianOpenMode = _settings.ObsidianDefaultOpenMode;
                if (Utils.IsKeyPressed(Keys.ControlKey))
                    obsidianOpenMode = _settings.ObsidianCtrlOpenMode;
                if (Utils.IsKeyPressed(Keys.ShiftKey))
                    obsidianOpenMode = _settings.ObsidianShiftOpenMode;
                if (Utils.IsKeyPressed(Keys.Menu))
                    obsidianOpenMode = _settings.ObsidianAltOpenMode;
                if (Utils.IsKeyPressed(Keys.LWin) || Utils.IsKeyPressed(Keys.RWin))
                    obsidianOpenMode = _settings.ObsidianWinOpenMode;

                if (obsidianOpenMode is ObsidianOpenMode.NewWindow)
                {
                    await OpenFileInNewWindow(vault, filepath);
                }
                else
                {
                    string openmode = obsidianOpenMode switch
                    {
                        ObsidianOpenMode.CurrentTab => "false",
                        ObsidianOpenMode.NewTab => "tab",
                        //ObsidianOpenMode.NewWindow => "window",
                        ObsidianOpenMode.NewPane => "split",
                        ObsidianOpenMode.HoverPopover => "popover",
                        ObsidianOpenMode.VaultAndNewWindow => "window",
                        _ => throw new ArgumentException()
                    };
                    Process.Start($"obsidian://advanced-uri?vault={PercentEncode(vault)}&filepath={PercentEncode(filepath)}&openmode={openmode}");
                }
            }
        }

        private async Task OpenFileInNewWindow(string vault, string filepath)
        {
            List<WindowVisualState> states = Utils.EnumerateProcessWindowHandles("Obsidian", "Chrome_WidgetWin_1").Select(w => new WindowVisualState(w)).ToList();
            Process.Start($"obsidian://advanced-uri?vault={PercentEncode(vault)}&filepath={PercentEncode(filepath)}&openmode=window");

            Stopwatch stopwach = Stopwatch.StartNew();
            do
            {
                await Task.Delay(50);
                // This is not precise enough. If the vault hasn't been opend before, Obsidian will create two or more windows.
                // But that case is hard to detect.
                if (Utils.EnumerateProcessWindowHandles("Obsidian", "Chrome_WidgetWin_1").Count() > states.Count)
                {
                    // 250~477ms
                    Debug.WriteLine($"Found new window after {stopwach.ElapsedMilliseconds}ms");
                    break;
                }
            } while (stopwach.ElapsedMilliseconds < 10000);

            foreach (WindowVisualState state in states)
            {
                state.Restore();
            }

            /*
            // If not specify Chrome_WidgetWin_1, we will get Chrome_WidgetWin_0.
            HWND newWindow = Utils.EnumerateProcessWindowHandles("Obsidian", "Chrome_WidgetWin_1").Where(x => !states.Select(x => x.Handle).Contains(x)).First();
            Debug.WriteLine($"New window: {new WindowVisualState(newWindow)}");

            PInvoke.BringWindowToTop(newWindow);
            */
        }

        private static string GetVaultName(string path)
        {
            return Path.GetFileName(path);
        }

        private static string GetFileVaultPath(string path)
        {
            // "This constructor does not check if a directory exists."
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                if (Directory.Exists(directory.FullName + @"\.obsidian"))
                {
                    return directory.FullName;
                }
            }

            directory = directory.Parent;
            while (directory is not null)
            {
                if (Directory.Exists(directory.FullName + @"\.obsidian"))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }

        private static bool IsFileInVault(string path)
        {
            return GetFileVaultPath(path) is not null;
        }

        private void OpenFileByFallback(string path)
        {
            // Process.Start() will not expand environment variables
            string editor = Environment.ExpandEnvironmentVariables(_settings.FallbackMarkdownEditor);

            // the filename and arguments must be provided seperately when calling Process.Start()
            Process.Start(editor, String.Format(_settings.FallbackMarkdownEditorArguments, $@"""{path}"""));
        }

        private async Task OpenFileInRecent(string path)
        {
            // hard link stays valid when the source file is deleted;
            // symbolic link requires SeCreateSymbolicLinkPrivilege;
            // so we use junction for simplicity

            string path_in_recent;

            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                // find the Markdown file with the minimum edit distance from the directory name
                Fastenshtein.Levenshtein edit_distance = new Fastenshtein.Levenshtein(directory.Name);
                int min_distance = int.MaxValue;
                FileInfo most_similar_file = null;
                foreach (FileInfo file in directory.EnumerateFiles("*.md"))
                {
                    int distance = edit_distance.DistanceFrom(file.Name);
                    if (distance < min_distance)
                    {
                        most_similar_file = file;
                        min_distance = distance;
                    }
                }

                path_in_recent = CreateLinkInRecent(directory, true);

                if (most_similar_file is null is false)
                    path_in_recent += '\\' + most_similar_file.Name;
            }
            else
            {
                path_in_recent = CreateLinkInRecent(directory.Parent, false) + '\\' + directory.Name;
            }

            await OpenFileInVault(path_in_recent, _settings.RecentVault);
        }

        private static string FormatLinkName(string prefixed_name, bool explicitDirectory)
        {
            string s = "";

            if (prefixed_name[0] == '.')
                s += ' ';

            s += prefixed_name.Replace('\\', '＼');

            if (explicitDirectory)
                s += "＼";

            return s;
        }

        private string CreateLinkInRecent(DirectoryInfo directory, bool explicitDirectory)
        {
            var provider = ReparsePointFactory.Provider;
            DirectoryInfo recent = new DirectoryInfo(_settings.RecentVault);

            string dir_target_same = null;
            List<string> conflict_dirs = new List<string>();
            List<string> conflict_targets = new List<string>();
            bool TestTarget(string path)
            {
                ReparseLink link = provider.GetLink(path);
                if (link.Target is null)
                    return false;
                /*
                if (!Directory.Exists(link.Target))
                {
                    Directory.Delete(path);
                    return false;
                }
                */

                if (path.EndsWith($"\\{directory.Name}") || path.EndsWith($"＼{directory.Name}"))
                {
                    if (link.Target == directory.FullName)
                    {
                        dir_target_same = path;
                        return true;
                    }

                    if (!Directory.Exists(link.Target))
                    {
                        Directory.Delete(path);
                        return false;
                    }

                    conflict_dirs.Add(path);
                    conflict_targets.Add(link.Target);
                }
                else
                {
                    // If the parent directory of a directory is already indexed by Obsidian, the directory won't be indexed before restarting Obsidian.
                    // The same is true even for a directory whose subdirectories are already indexed.

                    if (directory.FullName.StartsWith(link.Target))
                    {
                        if (path.EndsWith("＼"))
                        {
                            dir_target_same = path + directory.FullName.Substring(link.Target.Length);
                            return true;
                        }
                        else
                        {
                            Directory.Delete(path);
                            return false;
                        }
                    }

                    if (link.Target.StartsWith(directory.FullName))
                    {
                        Directory.Delete(path);
                        return false;
                    }
                }
                return false;
            }
            string formatted_name = FormatLinkName(directory.Name, false);
            if (Directory.Exists(recent.FullName + $@"\{formatted_name}") && TestTarget(recent.FullName + $@"\{formatted_name}"))
                return dir_target_same;
            string formatted_name_explicit = FormatLinkName(directory.Name, true);
            if (Directory.Exists(recent.FullName + $@"\{formatted_name_explicit}") && TestTarget(recent.FullName + $@"\{formatted_name_explicit}"))
                return dir_target_same;
            foreach (DirectoryInfo dir in recent.EnumerateDirectories())  // $"*＼{directory.Name}"
            {
                // reduce unnecessary IO operations
                if (dir.Name == ".obsidian" || dir.Name == formatted_name || dir.Name == formatted_name_explicit)
                    continue;

                if (TestTarget(dir.FullName))
                    return dir_target_same;
            }

            conflict_targets.Add(directory.FullName);
            List<string> prefixed_dirs = PathPrefix.PathPrefixed(conflict_targets);
            string path_in_recent = recent.FullName + '\\';
            void Move(string source, string dest)
            {
                if (source == dest)
                    return;
                Directory.Move(source, dest);
            }
            for (int i = 0; i < conflict_dirs.Count; i++)
            {
                // may conflict?
                Move(conflict_dirs[i], path_in_recent + FormatLinkName(prefixed_dirs[i], conflict_dirs[i].EndsWith("＼")));
                /*
                Directory.Delete(conflict_dirs[i]);
                provider.CreateLink(path_in_recent + prefixed_dirs[i].Replace('\\', '＼'), conflict_targets[i], LinkType.Junction);
                */
            }

            // Recent = dirs + .obsidian
            if (recent.GetDirectories().Length > _settings.RecentVaultSubdirectoriesLimit)
            {
                (from f in recent.GetDirectories()
                 orderby f.LastWriteTime ascending
                 where f.Name != ".obsidian"
                 select f).First().Delete();
            }

            path_in_recent += FormatLinkName(prefixed_dirs.Last(), explicitDirectory);
            provider.CreateLink(path_in_recent, directory.FullName, LinkType.Junction);
            return path_in_recent;
        }
    }
}