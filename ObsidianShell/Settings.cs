using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace ObsidianShell
{
    public enum OpenMode
    {
        VaultFallback,
        VaultRecent,
        Recent
    }    

    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonConverter(typeof(StringEnumConverter))]
        public OpenMode OpenMode { get; set; } = OpenMode.VaultFallback;

        public string FallbackMarkdownEditor { get; set; } = "notepad";

        public string FallbackMarkdownEditorArguments { get; set; } = "{0}";

        public string RecentVault { get; set; }

        public int RecentVaultSubdirectoriesLimit { get; set; } = 10;

        private static string GetPath()
        {
            string path = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "Settings.json");
            if (File.Exists(path))
                return path;
            
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Chaoses Ib", "ObsidianShell", "Settings.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(new Settings()));
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
