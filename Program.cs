using Game.Properties;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace Game
{
    internal class Program
    {
        public static int Language { get; set; }
        public static int Player { get; set; } = 1;
        public static string StartWord { get; set; } = string.Empty;
        public static Dictionary<char, int> WordLetters { get; set; } = new();
        public static LinkedList<string> Words { get; set; } = new();
        public const int Seconds = 20;
        public static bool TimeExpired = false;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            SetLanguage(0);
            var MenuItems = GetMenuItems();

            while (true)
            {
                int selected = Menu(MenuItems, Lang.Menu_Title);

                switch (selected)
                {
                    case 0: PlayGame(); break;
                    case 1: PrintRules(); break;
                    case 2: ChoiceLanguage(); MenuItems = GetMenuItems(); break;
                    case 3: return;
                    default: return;
                }

                Console.WriteLine(Lang.PressAnyKey);
                Console.ReadKey(true);
            }
        }

        static int Menu(string[] items, string title)
        {
            int selected = 0;

            void DisplayMenu()
            {
                Console.Clear();
                Console.WriteLine($"=== {title} ===\n");
                for (int i = 0; i < items.Length; i++)
                {
                    if (i == selected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {items[i]}");
                        Console.ResetColor();
                    }
                    else Console.WriteLine($"  {items[i]}");
                }
            }

            while (true)
            {
                DisplayMenu();
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) selected = (selected - 1 + items.Length) % items.Length;
                else if (key == ConsoleKey.DownArrow) selected = (selected + 1) % items.Length;
                else if (key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.WriteLine($"=== {items[selected]} ===\n");
                    return selected;
                }
                else if (key == ConsoleKey.Escape) return -1;
            }
        }

        static void PlayGame()
        {
            Console.Clear();
            Console.Write($"{Lang.Player} {Player} {Lang.Enter_Start_Word}: ");
            StartWord = Console.ReadLine()?.ToLower() ?? "";

            if (!CheckStartWord(StartWord))
            {
                PrintError($"\n{Lang.Start_Word_Error}\n");
                return;
            }

            Words.Clear();
            WordLetters.Clear();

            Words.AddLast(StartWord);
            GetLetterCount();
            SwitchPlayer();

            while (true)
            {
                Console.Write($"\n{Lang.Player} {Player} {Lang.Enter_Word}: ");
                string PlayerWord = ReadLineWithTimer();

                if (TimeExpired) break;

                if (!CheckPlayerWord(PlayerWord))
                {
                    Words.Clear();
                    WordLetters.Clear();
                    break;
                }

                Words.AddLast(PlayerWord);
                SwitchPlayer();
            }

            SwitchPlayer();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{Lang.Player_Win} {Player}!\n");
            Console.ResetColor();
            Player = 1;
            return;
        }

        static string ReadLineWithTimer()
        {
            StringBuilder builder = new();
            TimeExpired = false;

            System.Timers.Timer timer = new(Seconds * 1000)
            {
                AutoReset = false
            };

            timer.Elapsed += (s, e) =>
            {
                TimeExpired = true;
                PrintError($"\n\n{Lang.Time_Over_Err}");
            };

            timer.Start();

            while (!TimeExpired)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Remove(builder.Length - 1, 1);
                            Console.Write("\b \b");
                        }
                        continue;
                    }

                    if (!char.IsControl(key.KeyChar))
                    {
                        builder.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
                Thread.Sleep(50);
            }

            timer.Stop();
            Console.WriteLine();

            return TimeExpired ? string.Empty : builder.ToString().ToLower();
        }

        static bool CheckPlayerWord(string word)
        {
            if (word == string.Empty)
            {
                PrintError($"\n{Lang.Empty_String_Err}\n");
                return false;
            }

            if (word.Length > StartWord.Length)
            {
                PrintError($"\n{Lang.Long_Player_Word_Err}\n");
                return false;
            }

            if (Words.Contains(word))
            {
                PrintError($"\n{Lang.Word_Been_Err}\n");
                return false;
            }

            foreach (var ch in word)
            {
                if (!WordLetters.ContainsKey(ch))
                {
                    PrintError($"\n{Lang.No_Letter_Err} {ch} {Lang.Initial_Word}!\n");
                    return false;
                }
                if (WordLetters[ch] < word.Count(c => c == ch))
                {
                    PrintError($"\n{Lang.Many_Letters_Err} {ch}{Lang.In_Word}!\n");
                    return false;
                }
            }

            return true;
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void SwitchPlayer() => Player = Player == 1 ? 2 : 1;

        static bool CheckStartWord(string word) => (word.Length >= 8 && word.Length <= 30);

        static void GetLetterCount()
        {
            WordLetters.Clear();
            foreach (var ch in StartWord)
            {
                if (!WordLetters.ContainsKey(ch)) WordLetters.Add(ch, 1);
                else WordLetters[ch]++;
            }
        }

        static void PrintRules() => Console.WriteLine(Lang.Rules_Text);

        static void ChoiceLanguage()
        {
            string[] languages = { Lang.Ru_Lang, Lang.En_Lang };
            int selected = Menu(languages, Lang.Menu_Language);

            if (selected == 0) SetLanguage(0);
            else if (selected == 1) SetLanguage(1);
        }

        static string[] GetMenuItems()
        {
            return
            [
                Lang.Menu_Game,
                Lang.Menu_Rules,
                Lang.Menu_Language,
                Lang.Menu_Exit
            ];
        }

        static void SetLanguage(int lang)
        {
            Language = lang;
            CultureInfo culture = Language == 0
                ? new CultureInfo("ru-RU")
                : new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = culture;
            Console.OutputEncoding = Encoding.UTF8;
        }
    }
}