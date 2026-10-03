
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mushroomarizer {
    class Mushroomarizer {
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        static readonly Random random = new Random();

        static readonly string[] hiddenContents = {
            "desktop.ini",
            "icon.ico"
        };

        static bool ForceDeleteFile(string filePath) {
            try {
                FileAttributes attrs = File.GetAttributes(filePath) | FileAttributes.Normal;
                File.SetAttributes(filePath, attrs & ~FileAttributes.ReadOnly);
                File.Delete(filePath);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        static string[] MushroomFiles(string folderPath) {
            return new string[] {
                folderPath + @"\desktop.ini",
                folderPath + @"\icon.ico",
                folderPath + @"\.hidden"
            };
        }

        static bool IsMushroomarized(string folderPath) {
            string hidden = folderPath + @"\.hidden";
            return File.Exists(hidden) && File.ReadAllLines(hidden).SequenceEqual(hiddenContents);
        }

        static bool HasForeignIconFiles(string folderPath) {
            return !IsMushroomarized(folderPath) && MushroomFiles(folderPath).Any(File.Exists);
        }

        static void NoMoreMushrooms(string folderPath) {
            // Clearing out icon files
            foreach (string file in MushroomFiles(folderPath)) {
                if (File.Exists(file)) {
                    ForceDeleteFile(file);
                }
            }
        }

        static void ChangeFolderIcon(string folderPath, string iconPath) {
            string desktopini = folderPath + @"\desktop.ini";
            string iconico = folderPath + @"\icon.ico";
            string hidden = folderPath + @"\.hidden";

            NoMoreMushrooms(folderPath);

            // File attributes for all files
            FileAttributes attrs =
                FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;

            // Copy icon
            File.Copy(iconPath, iconico);
            File.SetAttributes(iconico, attrs);

            // Create desktop.ini
            string[] desktopiniContents = {
                "[.ShellClassInfo]",
                "IconResource=icon.ico,0"
            };

            File.WriteAllLines(desktopini, desktopiniContents);
            File.SetAttributes(desktopini, attrs);

            // Create .hidden
            File.WriteAllLines(hidden, hiddenContents);
            File.SetAttributes(hidden, attrs);
        }

        static List<string> GetDesktopFolders() {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DirectoryInfo desktop = new DirectoryInfo(desktopPath);

            List<string> folders = new List<string>();
            foreach (DirectoryInfo folder in desktop.GetDirectories()) {
                folders.Add(folder.FullName);
            }
            return folders;
        }

        static List<string> GetDesktopShortcuts() {
            // Collect all .lnk files on desktop
            List<string> shortcuts = new List<string>();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach (string file in Directory.GetFiles(desktopPath, "*.lnk")) {
                shortcuts.Add(file);
            }

            string publicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            foreach (string file in Directory.GetFiles(publicDesktopPath, "*.lnk")) {
                shortcuts.Add(file);
            }

            return shortcuts;
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
            Console.WriteLine("Mushroomarizing...");

            // Icons are copied next to the executable, which isn't always the working directory
            string iconsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons");

            // Get all .ico files in icons folder
            string[] mushrooms = Directory.Exists(iconsPath) ? Directory.GetFiles(iconsPath, "*.ico") : new string[0];
            if (mushrooms.Length == 0) {
                Console.WriteLine("No mushrooms found in " + iconsPath);
                return;
            }

            List<string> folders = GetDesktopFolders();
            foreach (string folder in folders) {
                try {
                    if (HasForeignIconFiles(folder)) {
                        Console.WriteLine("Skipping " + folder + " (it already has its own icon)");
                        continue;
                    }

                    Console.WriteLine("Mushroomarizing " + folder);

                    // Pick a random mushroom
                    string iconPath = mushrooms[random.Next(mushrooms.Length)];

                    ChangeFolderIcon(folder, iconPath);
                    File.SetAttributes(folder, File.GetAttributes(folder) | FileAttributes.ReadOnly);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            RefreshIcons();

            Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
        }

        static void Unmushroomarize() {
            Console.WriteLine("Unmushroomarizing...");

            List<string> folders = GetDesktopFolders();
            foreach (string folder in folders) {
                try {
                    // Leave alone folders we haven't touched
                    if (!IsMushroomarized(folder)) {
                        continue;
                    }

                    Console.WriteLine("Unmushroomarizing " + folder);

                    NoMoreMushrooms(folder);
                    File.SetAttributes(folder, File.GetAttributes(folder) & ~FileAttributes.ReadOnly);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }

            RefreshIcons();

            Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
        }

        static void RefreshIcons() {
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
