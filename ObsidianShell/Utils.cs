using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ObsidianShell
{
    internal class WindowVisualState
    {
        public HWND Handle { get; }
        //private bool _visibility;
        private bool _minimized;
        private HWND _prevWindow;

        public WindowVisualState(HWND handle)
        {
            Handle = handle;
            //_visibility = PInvoke.IsWindowVisible(handle);
            _minimized = PInvoke.IsIconic(handle);
            _prevWindow = GetPrevVisibleWindow(handle);
        }

        private static HWND GetPrevVisibleWindow(HWND handle)
        {
            HWND nextWindow = handle;
            while ((nextWindow = PInvoke.GetWindow(nextWindow, GET_WINDOW_CMD.GW_HWNDPREV)) != default)
            {
                if (PInvoke.IsWindowVisible(nextWindow))
                    return nextWindow;
            }
            return default;
        }

        public void Restore()
        {
            WindowVisualState newState = new(Handle);
            if (//newState._visibility != _visibility ||
                newState._minimized != _minimized ||
                newState._prevWindow != _prevWindow
                )
            {
                SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;
                //flags |= _visibility ? SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW : SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW;
                // SetWindowPos will move hWnd below hWndInsertAfter
                PInvoke.SetWindowPos(Handle, _prevWindow, 0, 0, 0, 0, flags);

                if (_minimized)
                    PInvoke.ShowWindow(Handle, SHOW_WINDOW_CMD.SW_MINIMIZE);

                Debug.WriteLine($"{this}\n-> {newState}\n-> {new WindowVisualState(Handle)}");
            }
        }

        public override string ToString()
        {
            return @$"{Handle.Value}({Utils.GetClassName(Handle)}:{Utils.GetWindowText(Handle)}) {""/*_visibility*/} {_minimized} {_prevWindow.Value}({Utils.GetClassName(_prevWindow)}:{Utils.GetWindowText(_prevWindow)})";
        }
    }

    internal class Utils
    {
        internal static string GetClassName(HWND hWnd)
        {
            unsafe
            {
                char* className = stackalloc char[256];
                PInvoke.GetClassName(hWnd, className, 256);
                return new string(className);
            }
        }        

        internal static string GetWindowText(HWND hWnd)
        {
            unsafe
            {
                char* buffer = stackalloc char[256];
                PInvoke.SendMessage(hWnd, PInvoke.WM_GETTEXT, 256, (nint)buffer);
                return new(buffer);
            }
        }

        public static IEnumerable<HWND> EnumerateProcessWindowHandles(string friendlyProcessName, string className = null)
        {
            List<HWND> handles = new();
            foreach (Process process in Process.GetProcessesByName(friendlyProcessName))
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    PInvoke.EnumThreadWindows((uint)thread.Id, (hWnd, lParam) => {
                        if (className is null || GetClassName(hWnd) == className)
                        {
                            handles.Add(hWnd);
                        }
                        return true;
                    }, IntPtr.Zero);
                }
            }
            return handles;
        }

        public static bool IsKeyPressed(Keys key)
        {
            return (PInvoke.GetAsyncKeyState((int)key) & 0x8000) != 0;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fromPath"/> or <paramref name="toPath"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
