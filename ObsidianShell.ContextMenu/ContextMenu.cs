using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ObsidianShell.ContextMenu
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class ContextMenu : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var open_in_obsidian_recent = new ToolStripMenuItem
            {
                Text = "Open in Obsidian (&B)",
                Image = Resources.Obsidian_16
            };
            open_in_obsidian_recent.Click += (sender, args) => OpenInObsidianRecent();
            menu.Items.Add(open_in_obsidian_recent);

            return menu;
        }

        private void OpenInObsidianRecent()
        {
            try
            {
                foreach (string path in SelectedItemPaths)
                {
                    string cli = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\ObsidianShell.CLI.exe";
                    Process.Start(cli, $@"""{path}""");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ObsidianShell.ContextMenu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
