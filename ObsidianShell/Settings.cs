using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

namespace ObsidianShell
{
    public enum OpenMode
    {
        VaultFallback = 0,
        VaultRecent = 1,
        Recent = 2
    }

    public enum ObsidianOpenMode
    {
        CurrentTab = 0,
        NewTab = 1,
        NewWindow = 2,
        VaultAndNewWindow = 5,
        NewPane = 3,
        HoverPopover = 4,
    }

    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public OpenMode OpenMode { get; set; } = OpenMode.VaultFallback;

        public string FallbackMarkdownEditor { get; set; } = "notepad";
        public string FallbackMarkdownEditorArguments { get; set; } = "{0}";

        public string RecentVault { get; set; }
        public int RecentVaultSubdirectoriesLimit { get; set; } = 10;

        public bool EnableAdvancedURI { get; set; } = false;
        public ObsidianOpenMode ObsidianDefaultOpenMode { get; set; } = ObsidianOpenMode.NewTab;
        public ObsidianOpenMode ObsidianCtrlOpenMode { get; set; } = ObsidianOpenMode.NewPane;
        public ObsidianOpenMode ObsidianShiftOpenMode { get; set; } = ObsidianOpenMode.NewWindow;
        public ObsidianOpenMode ObsidianAltOpenMode { get; set; } = ObsidianOpenMode.CurrentTab;
        public ObsidianOpenMode ObsidianWinOpenMode { get; set; } = ObsidianOpenMode.HoverPopover;

        private static string GetPath()
        {
            string portablePath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Settings.json");
            if (File.Exists(portablePath))
                return portablePath;
            
            string installedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chaoses Ib", "ObsidianShell", "Settings.json");
            if (File.Exists(installedPath))
                return installedPath;

            // Portable version is not released with an empty settings file, because users may overwrite existing settings during upgrade.
            // So we need to try to create at the portable path first.
            try
            {
                File.WriteAllText(portablePath, "{}");
                return portablePath;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to create settings file at {portablePath}:\n{e}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            File.WriteAllText(installedPath, "{}");
            return installedPath;
        }

        public static Settings Load()
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(GetPath()));
        }

        public void Save()
        {
            File.WriteAllText(GetPath(), JsonConvert.SerializeObject(this));
        }
    }
}
