
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Mushroomarizer
{
  class Mushroomarizer
  {
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

    static bool ForceDeleteFile(string filePath)
    {
      try
      {
        FileAttributes attrs = File.GetAttributes(filePath) | FileAttributes.Normal;
        File.SetAttributes(filePath, attrs & ~FileAttributes.ReadOnly);
        File.Delete(filePath);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        return false;
      }

      return true;
    }

    static void NoMoreMushrooms(string folderPath, bool createBackup = false)
    {
      string desktopini = folderPath + @"\desktop.ini";
      string iconico = folderPath + @"\icon.ico";
      string hidden = folderPath + @"\.hidden";

      // Clearing out any existing icons
      foreach (string file in new string[] { desktopini, iconico, hidden })
      {
        if (File.Exists(file))
        {
          if (createBackup)
          {
            // Make backup of file
            string backup = file + ".bak";
            if (File.Exists(backup))
            {
              ForceDeleteFile(backup);
            }

            // Copy only contents of file
            File.Copy(file, backup);
            File.SetAttributes(backup, FileAttributes.Normal);
          }

          ForceDeleteFile(file);
        }
      }
    }

    static void ChangeFolderIcon(string folderPath, string iconPath)
    {
      string desktopini = folderPath + @"\desktop.ini";
      string iconico = folderPath + @"\icon.ico";
      string hidden = folderPath + @"\.hidden";

      NoMoreMushrooms(folderPath, false);

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
      string[] hiddenContents = {
        "desktop.ini",
        "icon.ico"
      };

      File.WriteAllLines(hidden, hiddenContents);
      File.SetAttributes(hidden, attrs);
    }

    static List<string> GetDesktopFolders()
    {
      List<string> folders = new List<string>();
      string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      DirectoryInfo desktop = new DirectoryInfo(desktopPath);
      foreach (DirectoryInfo folder in desktop.GetDirectories())
      {
        folders.Add(folder.FullName);
      }
      return folders;
    }

    static void Main(string[] args)
    {
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

      while (true)
      {
        Console.Write("Enter option: ");
        string input = Console.ReadLine();
        if (input == null)
        {
          Console.WriteLine("Invalid option");
          continue;
        }

        switch (input)
        {
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

    static void Mushroomarize()
    {
      Console.WriteLine("Mushroomarizing...");

      string appPath = Directory.GetCurrentDirectory();

      // Get all .ico files in icons folder
      string[] mushrooms = Directory.GetFiles(appPath + @"\icons", "*.ico");

      List<string> folders = GetDesktopFolders();
      foreach (string folder in folders)
      {
        Console.WriteLine("Mushroomarizing " + folder);

        // Pick a random mushroom
        string iconPath = mushrooms[(new Random()).Next(mushrooms.Length)];

        ChangeFolderIcon(folder, iconPath);
        File.SetAttributes(folder, FileAttributes.Normal | FileAttributes.ReadOnly);

        RefreshFolder(folder);
      }

      Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
    }

    static void Unmushroomarize()
    {
      Console.WriteLine("Unmushroomarizing...");

      List<string> folders = GetDesktopFolders();
      foreach (string folder in folders)
      {
        Console.WriteLine("Unmushroomarizing " + folder);

        NoMoreMushrooms(folder);
        RefreshFolder(folder);

        File.SetAttributes(folder, FileAttributes.Normal);
      }

      Console.WriteLine("Done! Please wait a few seconds for the changes to take effect.");
    }

    static void RefreshFolder(string folderPath)
    {
      SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
    }
  }
}
