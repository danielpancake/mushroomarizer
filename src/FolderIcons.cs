using System.IO;
using System.Linq;
using System.Text;

namespace Mushroomarizer {
    static class FolderIcons {
        const string IconResource = "IconResource=";

        public static bool IsMushroomarized(string folder) {
            string desktopIni = DesktopIni(folder);
            return File.Exists(desktopIni) && File.ReadAllLines(desktopIni).Any(line =>
                line.StartsWith(IconResource) && Mushroomarizer.IsMushroom(line.Substring(IconResource.Length)));
        }

        public static bool HasOwnIcon(string folder) {
            return File.Exists(DesktopIni(folder)) && !IsMushroomarized(folder);
        }

        public static void Mushroomarize(string folder, string iconPath) {
            string desktopIni = DesktopIni(folder);
            if (File.Exists(desktopIni)) {
                File.SetAttributes(desktopIni, FileAttributes.Normal);
            }

            File.WriteAllLines(desktopIni, new[] { "[.ShellClassInfo]", IconResource + iconPath + ",0" }, Encoding.Unicode);
            File.SetAttributes(desktopIni, FileAttributes.Hidden | FileAttributes.System);

            File.SetAttributes(folder, File.GetAttributes(folder) | FileAttributes.ReadOnly);
        }

        public static void Unmushroomarize(string folder) {
            string desktopIni = DesktopIni(folder);
            File.SetAttributes(desktopIni, FileAttributes.Normal);
            File.Delete(desktopIni);

            File.SetAttributes(folder, File.GetAttributes(folder) & ~FileAttributes.ReadOnly);
        }

        static string DesktopIni(string folder) {
            return Path.Combine(folder, "desktop.ini");
        }
    }
}
