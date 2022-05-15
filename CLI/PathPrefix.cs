using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObsidianCLI
{
    internal class PathPrefix
    {
        static bool AllStartWith(IEnumerable<string> paths, string prefix)
        {
            return paths.All(path => path.StartsWith(prefix));
        }

        static bool AllEndWith(IEnumerable<string> paths, string suffix)
        {
            return paths.All(path => path.EndsWith(suffix));
        }

        static string CommonPrefixOf(IEnumerable<string> paths)
        {
            string prefix = paths.First();
            // range operator requires C# 8.0
            prefix = prefix.Substring(0, prefix.LastIndexOf('\\'));
            while (!AllStartWith(paths, prefix))
            {
                int pos = prefix.LastIndexOf('\\');
                if (pos == -1)
                    return "";
                prefix = prefix.Substring(0, pos);
            }
            return prefix + '\\';
        }

        static string CommonSuffixOf(IEnumerable<string> paths)
        {
            string suffix = paths.First();
            // range operator requires C# 8.0
            suffix = suffix.Substring(suffix.LastIndexOf('\\') + 1);
            while (!AllEndWith(paths, suffix))
            {
                int pos = suffix.IndexOf('\\');
                if (pos == -1)
                    return "";
                suffix = suffix.Substring(pos + 1);
            }
            return '\\' + suffix;
        }

        public static List<string> PathPrefixed(IEnumerable<string> paths)
        {
            string name = paths.First().Substring(paths.First().LastIndexOf('\\') + 1);
            if (paths.Count() == 1)
                return new List<string> { name };
            string prefix = CommonPrefixOf(paths);
            string suffix = CommonSuffixOf(paths);
            bool trival_suffix = suffix == $"\\{name}";

            List<string> prefixed = new List<string>();
            foreach (string path in paths)
            {
                string p = path.Substring(prefix.Length, Math.Max(path.Length - prefix.Length - suffix.Length, 0));
                if (p != "")
                {
                    if (!trival_suffix)
                        p += "\\…";
                    p += '\\';
                }
                prefixed.Add(p + name);
            }
            return prefixed;
        }
    }
}
