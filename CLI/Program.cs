using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Process.Start("obsidian://open");
                return;
            }

            string path = args[0];
            if (Directory.Exists(path) || IsFileInVault(path))
            {
                // WebUtility.UrlEncode() and Uri.EscapeDataString() will escape space with "+" rather than "%20"
                Process.Start($"obsidian://open?path={Uri.EscapeUriString(path)}");
            }
            else
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                string fallback = config.AppSettings.Settings["MarkdownFallback"]?.Value;
                if (fallback is null)
                {
                    fallback = "notepad";
                    config.AppSettings.Settings.Add("MarkdownFallback", fallback);
                    config.Save();
                }
                // Process.Start() will not expand environment variables
                fallback = Environment.ExpandEnvironmentVariables(fallback);

                string fallback_arguments = config.AppSettings.Settings["MarkdownFallbackArguments"]?.Value;
                if (fallback_arguments is null)
                {
                    fallback_arguments = "{0}";
                    config.AppSettings.Settings.Add("MarkdownFallbackArguments", fallback_arguments);
                    config.Save();
                }

                //MessageBox.Show(fallback + " " + String.Format(fallback_arguments, $@"""{path}"""));
                
                // the filename and arguments must be provided seperately when calling Process.Start()
                Process.Start(fallback, String.Format(fallback_arguments, $@"""{path}"""));
            }
        }

        static bool IsFileInVault(string path)
        {
            DirectoryInfo directory = Directory.GetParent(path);
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
    }
}
