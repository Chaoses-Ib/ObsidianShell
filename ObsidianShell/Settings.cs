using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;

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
        NewPane = 3,
        HoverPopover = 4
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
        public ObsidianOpenMode ObsidianDefaultOpenMode { get; set; } = ObsidianOpenMode.NewWindow;
        public ObsidianOpenMode ObsidianCtrlOpenMode { get; set; } = ObsidianOpenMode.NewPane;
        public ObsidianOpenMode ObsidianShiftOpenMode { get; set; } = ObsidianOpenMode.CurrentTab;
        public ObsidianOpenMode ObsidianAltOpenMode { get; set; } = ObsidianOpenMode.NewTab;
        public ObsidianOpenMode ObsidianWinOpenMode { get; set; } = ObsidianOpenMode.HoverPopover;

        private static string GetPath()
        {
            string path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Settings.json");
            if (File.Exists(path))
                return path;
            
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chaoses Ib", "ObsidianShell", "Settings.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}");
            }
            return path;
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
