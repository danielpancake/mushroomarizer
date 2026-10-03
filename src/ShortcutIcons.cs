using System;
using System.IO;

namespace Mushroomarizer {
    static class ShortcutIcons {
        static readonly dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

        public static bool IsMushroomarized(string shortcut) {
            return Mushroomarizer.IsMushroom(GetIcon(shortcut));
        }

        public static bool HasOwnIcon(string shortcut) {
            string icon = GetIcon(shortcut);
            return icon != "" && !Mushroomarizer.IsMushroom(icon);
        }

        public static void Mushroomarize(string shortcut, string iconPath) {
            SetIcon(shortcut, iconPath);
        }

        public static void Unmushroomarize(string shortcut) {
            SetIcon(shortcut, "");
        }

        static string GetIcon(string shortcut) {
            GetLink(shortcut).GetIconLocation(out string path);
            return path ?? "";
        }

        static void SetIcon(string shortcut, string iconPath) {
            dynamic link = GetLink(shortcut);
            link.SetIconLocation(iconPath, 0);
            link.Save();
        }

        static dynamic GetLink(string shortcut) {
            return shell.NameSpace(Path.GetDirectoryName(shortcut)).ParseName(Path.GetFileName(shortcut)).GetLink;
        }
    }
}
