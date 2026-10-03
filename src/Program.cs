using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mushroomarizer {
    class Program {
        static readonly string[] art = {
            @"                       .-'~~~-.",
            @"                     .'o  oOOOo`.",
            @"                    :~~~-.oOo   o`.",
            @"                     `. \ ~-.  oOOo.",
            @"                       `.; / ~.  OO:",
            @"                       .'  ;-- `.o.'",
            @"                      ,'  ; ~~--'~",
            @"                      ;  ;",
            @"{Green}_______\|/__________\\{/};{Green}_\\//___\|/________"
        };

        static readonly string[] title = {
            "",
            "",
            "",
            "   {Red}MUSHROOMARIZER{/}",
            "  {DarkGray}made by{/}",
            " {Yellow}danielpancake{/}"
        };

        static void Main(string[] args) {
            Console.OutputEncoding = new UTF8Encoding(false);

            if (args.Length > 0 || Console.IsInputRedirected) {
                Run(args.Length > 0 ? args[0].ToLowerInvariant() : "");
                return;
            }

            do {
                ClearToLogo();
                string command = Choose("Mushroomarize", "Unmushroomarize", "Exit");
                if (command == "Exit") {
                    return;
                }

                ClearToLogo();
                Run(command == "Mushroomarize" ? "on" : "off");
                Console.WriteLine();
            } while (Choose("Back", "Exit") == "Back");
        }

        static void Run(string command) {
            try {
                switch (command) {
                    case "on":
                        ShowDone("Done! Your desktop is mushroomarized.", Mushroomarizer.Mushroomarize());
                        break;
                    case "off":
                        ShowDone("Done! Your desktop is back to normal.", Mushroomarizer.Unmushroomarize());
                        break;
                    default:
                        Console.WriteLine("Usage: mushroomarizer [on | off]");
                        break;
                }
            } catch (Exception e) {
                Write(ConsoleColor.Red, e.Message + "\n");
            }
        }

        static void ShowDone(string message, List<string> failures) {
            Write(ConsoleColor.Green, message + "\n");
            failures.ForEach(failure => Write(ConsoleColor.Red, failure + "\n"));
        }

        static void ClearToLogo() {
            Console.Clear();
            foreach (string line in art) {
                WriteColored(line + "\n");
            }

            for (int i = 0; i < title.Length; i++) {
                string text = title[i].TrimStart(' ');
                Console.SetCursorPosition(title[i].Length - text.Length, i);
                WriteColored(text);
            }

            Console.SetCursorPosition(0, art.Length);
            Console.WriteLine();
        }

        static void WriteColored(string text) {
            string[] parts = Regex.Split(text, @"\{(\w+|/)\}");
            for (int i = 0; i < parts.Length; i++) {
                if (i % 2 == 0) {
                    Console.Write(parts[i]);
                } else if (parts[i] == "/") {
                    Console.ResetColor();
                } else {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), parts[i], true);
                }
            }
            Console.ResetColor();
        }

        static string Choose(params string[] options) {
            int selected = 0;
            bool chosen = false;

            while (true) {
                for (int i = 0; i < options.Length; i++) {
                    string option = (i + 1) + ". " + options[i];

                    if (i == selected) {
                        Write(ConsoleColor.Red, "> " + option + "\n");
                    } else {
                        Console.WriteLine("  " + option);
                    }
                }

                if (chosen) {
                    return options[selected];
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                int number = key.KeyChar - '0';
                if (number >= 1 && number <= options.Length) {
                    selected = number - 1;
                    chosen = true;
                }

                switch (key.Key) {
                    case ConsoleKey.Enter:
                        return options[selected];
                    case ConsoleKey.UpArrow:
                        selected = (selected + options.Length - 1) % options.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Length;
                        break;
                }

                Console.SetCursorPosition(0, Console.CursorTop - options.Length);
            }
        }

        static void Write(ConsoleColor color, string text) {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
    }
}
