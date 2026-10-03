using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mushroomarizer {
    static class Mushroomarizer {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(int wEventId, int uFlags, string dwItem1, IntPtr dwItem2);

        const int SHCNE_UPDATEITEM = 0x00002000;
        const int SHCNE_ASSOCCHANGED = 0x08000000;
        const int SHCNF_IDLIST = 0x0000;
        const int SHCNF_PATHW = 0x0005;

        static readonly Random random = new Random();

        static readonly string iconsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mushroomarizer");

        public static bool IsMushroom(string iconPath) {
            return Environment.ExpandEnvironmentVariables(iconPath)
                .StartsWith(iconsPath + @"\", StringComparison.OrdinalIgnoreCase);
        }

        // Returns a message for every folder or shortcut that failed
        public static List<string> Mushroomarize() {
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");

            string[] sources = Directory.Exists(sourcePath) ? Directory.GetFiles(sourcePath, "*.ico") : new string[0];
            if (sources.Length == 0) {
                throw new FileNotFoundException("No mushrooms found in " + sourcePath);
            }

            Directory.CreateDirectory(iconsPath);
            foreach (string source in sources) {
                string mushroom = Path.Combine(iconsPath, Path.GetFileName(source));
                if (!File.Exists(mushroom)) {
                    File.Copy(source, mushroom);
                }
            }
            string[] mushrooms = Directory.GetFiles(iconsPath, "*.ico");

            List<string> failures = new List<string>();
            ForEachPath(failures, GetDesktopFolders(), folder => {
                if (!FolderIcons.HasOwnIcon(folder)) {
                    FolderIcons.Mushroomarize(folder, mushrooms[random.Next(mushrooms.Length)]);
                }
            });

            ForEachPath(failures, GetDesktopShortcuts(), shortcut => {
                if (!ShortcutIcons.HasOwnIcon(shortcut)) {
                    ShortcutIcons.Mushroomarize(shortcut, mushrooms[random.Next(mushrooms.Length)]);
                }
            });

            RefreshIcons();
            return failures;
        }

        // Returns a message for every folder or shortcut that failed
        public static List<string> Unmushroomarize() {
            List<string> failures = new List<string>();
            ForEachPath(failures, GetDesktopFolders(), folder => {
                if (FolderIcons.IsMushroomarized(folder)) {
                    FolderIcons.Unmushroomarize(folder);
                }
            });

            ForEachPath(failures, GetDesktopShortcuts(), shortcut => {
                if (ShortcutIcons.IsMushroomarized(shortcut)) {
                    ShortcutIcons.Unmushroomarize(shortcut);
                }
            });

            // Anything that failed to restore still points to the stored mushrooms
            if (failures.Count == 0) {
                ForEachPath(failures, new[] { iconsPath }.Where(Directory.Exists), path => Directory.Delete(path, true));
            }

            RefreshIcons();
            return failures;
        }

        static void ForEachPath(List<string> failures, IEnumerable<string> paths, Action<string> action) {
            foreach (string path in paths) {
                try {
                    action(path);

                    // Manually redraw changed shortcut
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATHW, path, IntPtr.Zero);
                } catch (Exception e) {
                    failures.Add("Failed on " + path + ": " + e.Message);
                }
            }
        }

        static string[] GetDesktopFolders() {
            return Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        static IEnumerable<string> GetDesktopShortcuts() {
            return new[] { Environment.SpecialFolder.Desktop, Environment.SpecialFolder.CommonDesktopDirectory }
                .SelectMany(desktop => Directory.GetFiles(Environment.GetFolderPath(desktop), "*.lnk"));
        }

        static void RefreshIcons() {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, null, IntPtr.Zero);
        }
    }
}
