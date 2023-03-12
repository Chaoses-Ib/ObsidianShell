using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ObsidianShell.CLI
{
    internal class Program
    {
        static Settings _settings;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                MessageBox.Show(eventArgs.ExceptionObject.ToString(), "ObsidianShell.CLI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            if (args.Length == 0)
            {
                Process.Start("obsidian://open");
                return;
            }

            _settings = Settings.Load();

            Obsidian obsidian = new Obsidian(_settings);
            obsidian.OpenFile(args[0]);
        }
    }
}
