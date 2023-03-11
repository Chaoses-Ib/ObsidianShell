using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ObsidianShell.GUI
{
    class OpenModeView
    {
        public OpenMode OpenMode { get; set; }
        public string Description { get; set; }
        public bool EnableFallbackMarkdownEditor { get; set; }
        public bool EnableRecentVault { get; set; }
    }
    
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public Settings Settings { get; }
        
        public ObservableCollection<OpenModeView> OpenModeViews { get; }

        public OpenModeView CurrentOpenModeView {
            get => OpenModeViews[(int)Settings.OpenMode];
            set => Settings.OpenMode = value.OpenMode;
        }

        public MainViewModel()
        {
            Settings = Settings.Load();

            OpenModeViews = new ObservableCollection<OpenModeView>
            {
                new OpenModeView {
                    OpenMode = OpenMode.VaultFallback,
                    Description = "If the Markdown file you want to open is in a vault, open the vault, otherwise open the file using the fallback Markdown editor.",
                    EnableFallbackMarkdownEditor = true,
                    EnableRecentVault = false
                },
                new OpenModeView
                {
                    OpenMode = OpenMode.VaultRecent,
                    Description = "If the Markdown file you want to open is in a vault, open the vault, otherwise link the file's parent directory to the Recent vault and then open the file.",
                    EnableFallbackMarkdownEditor = false,
                    EnableRecentVault = true
                },
                new OpenModeView
                {
                    OpenMode = OpenMode.Recent,
                    Description = "Whether the Markdown file you want to open is in a vault, link its parent directory to the Recent vault and then open it.",
                    EnableFallbackMarkdownEditor = false,
                    EnableRecentVault = true
                }
            };
        }

        public void Apply()
        {
            Settings.Save();
        }
    }
}
