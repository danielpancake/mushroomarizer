using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mushroomarizer {
    class Mushroomarizer {
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

        static void Main(string[] args) {
            // Show user a console menu
            string[] menu = {
                "Mushroomarizer",
                "==============",
                "1. Mushroomarize",
                "2. Unmushroomarize",
                "3. Exit",
                "Made by @danielpancake",
                ""
            };

            Console.WriteLine(string.Join("\n", menu));

            while (true) {
                Console.Write("Enter option: ");
                string input = Console.ReadLine();
                if (input == null) {
                    // End of input, nothing more to read
                    return;
                }

                switch (input.Trim()) {
                    case "1":
                        Mushroomarize();
                        break;
                    case "2":
                        Unmushroomarize();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
                Console.WriteLine();
            }
        }

        static void Mushroomarize() {
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");

            string[] sources = Directory.Exists(sourcePath) ? Directory.GetFiles(sourcePath, "*.ico") : new string[0];
            if (sources.Length == 0) {
                Console.WriteLine("No mushrooms found in " + sourcePath);
                return;
            }

            Directory.CreateDirectory(iconsPath);
            foreach (string source in sources) {
                string mushroom = Path.Combine(iconsPath, Path.GetFileName(source));
                if (!File.Exists(mushroom)) {
                    File.Copy(source, mushroom);
                }
            }
            string[] mushrooms = Directory.GetFiles(iconsPath, "*.ico");

            Console.WriteLine("Mushroomarizing...");

            ForEachPath(GetDesktopFolders(), folder => {
                if (FolderIcons.HasOwnIcon(folder)) {
                    Console.WriteLine("Skipping " + folder + " (it already has its own icon)");
                    return;
                }

                Console.WriteLine("Mushroomarizing " + folder);
                FolderIcons.Mushroomarize(folder, mushrooms[random.Next(mushrooms.Length)]);
            });

            ForEachPath(GetDesktopShortcuts(), shortcut => {
                if (ShortcutIcons.HasOwnIcon(shortcut)) {
                    Console.WriteLine("Skipping " + shortcut + " (it already has its own icon)");
                    return;
                }

                Console.WriteLine("Mushroomarizing " + shortcut);
                ShortcutIcons.Mushroomarize(shortcut, mushrooms[random.Next(mushrooms.Length)]);
            });

            RefreshIcons();

            Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
        }

        static void Unmushroomarize() {
            Console.WriteLine("Unmushroomarizing...");

            bool foldersRestored = ForEachPath(GetDesktopFolders(), folder => {
                // Leave alone folders we haven't touched
                if (!FolderIcons.IsMushroomarized(folder)) {
                    return;
                }

                Console.WriteLine("Unmushroomarizing " + folder);
                FolderIcons.Unmushroomarize(folder);
            });

            bool shortcutsRestored = ForEachPath(GetDesktopShortcuts(), shortcut => {
                if (!ShortcutIcons.IsMushroomarized(shortcut)) {
                    return;
                }

                Console.WriteLine("Unmushroomarizing " + shortcut);
                ShortcutIcons.Unmushroomarize(shortcut);
            });

            // Anything that failed to restore still points to the stored mushrooms
            if (foldersRestored && shortcutsRestored) {
                ForEachPath(new[] { iconsPath }.Where(Directory.Exists), path => Directory.Delete(path, true));
            }

            RefreshIcons();

            Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
        }

        static bool ForEachPath(IEnumerable<string> paths, Action<string> action) {
            bool succeeded = true;
            foreach (string path in paths) {
                try {
                    action(path);

                    // Manually redraw changed shortcut
                    SHChangeNotify(SHCNE_UPDATEITEM, SHCNF_PATHW, path, IntPtr.Zero);
                } catch (Exception e) {
                    Console.WriteLine("Failed on " + path + ": " + e.Message);
                    succeeded = false;
                }
            }
            return succeeded;
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
