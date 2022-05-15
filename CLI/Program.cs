using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NCode.ReparsePoints;

namespace ObsidianCLI
{
    internal class Program
    {
        static Configuration config;
        
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
                MessageBox.Show(eventArgs.ExceptionObject.ToString(), "ObsidianCLI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            if (args.Length == 0)
            {
                Process.Start("obsidian://open");
                return;
            }
            
            string path = args[0];

            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            string open_mode = GetConfigValueOr("OpenMode", "VaultFallback");
            switch (open_mode)
            {
                case "VaultFallback":
                    {
                        if (IsFileInVault(path))
                        {
                            OpenFileInVault(path);
                        }
                        else
                        {
                            OpenFileByFallback(path);
                        }
                        break;
                    }
                case "VaultRecent":
                    {
                        if (IsFileInVault(path))
                        {
                            OpenFileInVault(path);
                        }
                        else
                        {
                            OpenFileInRecent(path);
                        }
                        break;
                    }
                case "Recent":
                    {
                        OpenFileInRecent(path);
                        break;
                    }
            }
        }

        static string GetConfigValue(string key)
        {
            string value = config.AppSettings.Settings[key]?.Value;
            if (value is null)
            {
                MessageBox.Show($"Config value {key} not found", "ObsidianCLI", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return value;
        }

        static string GetConfigValueOr(string key, string default_value)
        {
            string value = config.AppSettings.Settings[key]?.Value;
            if (value is null)
            {
                return AddConfigElement(key, default_value);
            }
            return value;
        }

        static string AddConfigElement(string key, string value)
        {
            config.AppSettings.Settings.Add(key, value);
            config.Save();
            return value;
        }

        static bool IsFileInVault(string path)
        {
            // "This constructor does not check if a directory exists."
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                if (Directory.Exists(directory.FullName + @"\.obsidian"))
                {
                    return true;
                }
            }

            directory = directory.Parent;
            while (directory is null is false)  // `is not null` requires C# 9.0
            {
                if (Directory.Exists(directory.FullName + @"\.obsidian"))
                {
                    return true;
                }
                directory = directory.Parent;
            }
            return false;
        }

        static void OpenFileInVault(string path)
        {
            // WebUtility.UrlEncode() replaces space with "+" instead of "%20"
            // Uri.EscapeUriString() replaces space with "%20" but does not replace most reserved characters (!#$&'()+,;=@[])
            Process.Start($"obsidian://open?path={Uri.EscapeDataString(path)}");
        }

        static void OpenFileByFallback(string path)
        {
            string fallback = GetConfigValueOr("MarkdownFallback", "notepad");
            // Process.Start() will not expand environment variables
            fallback = Environment.ExpandEnvironmentVariables(fallback);

            string fallback_arguments = GetConfigValueOr("MarkdownFallbackArguments", "{0}");

            //MessageBox.Show(fallback + " " + String.Format(fallback_arguments, $@"""{path}"""));

            // the filename and arguments must be provided seperately when calling Process.Start()
            Process.Start(fallback, String.Format(fallback_arguments, $@"""{path}"""));
        }

        static void OpenFileInRecent(string path)
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

            OpenFileInVault(path_in_recent);
        }

        static string FormatLinkName(string prefixed_name, bool explicitDirectory)
        {
            string s = "";
            
            if (prefixed_name[0] == '.')
                s += ' ';
            
            s += prefixed_name.Replace('\\', '＼');

            if (explicitDirectory)
                s += "＼";

            return s;
        }

        static string CreateLinkInRecent(DirectoryInfo directory, bool explicitDirectory)
        {
            var provider = ReparsePointFactory.Provider;
            DirectoryInfo recent = new DirectoryInfo(GetConfigValue("RecentVault"));

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

                if (path.EndsWith($"＼{directory.Name}"))
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
                    
                    if (directory.FullName.StartsWith(link.Target)) {
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

            path_in_recent += FormatLinkName(prefixed_dirs.Last(), explicitDirectory);
            provider.CreateLink(path_in_recent, directory.FullName, LinkType.Junction);
            return path_in_recent;
        }
    }
}
