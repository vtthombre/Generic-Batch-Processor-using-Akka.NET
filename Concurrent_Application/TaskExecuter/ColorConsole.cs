using System;

namespace TaskExecuter
{
    public static class ColorConsole
    {
        public static void WriteLineGreen(string message, params object[] args)
        {
            Write(ConsoleColor.Green, string.Format(message, args));
        }
        public static void WriteLineBlue(string message, params object[] args)
        {
            Write(ConsoleColor.DarkBlue, string.Format(message, args));
        }
        public static void WriteLineYellow(string message, params object[] args)
        {
            Write(ConsoleColor.Yellow, string.Format(message, args));
        }

        public static void WriteLineRed(string message, params object[] args)
        {
            Write(ConsoleColor.Red, string.Format(message, args));
        }

        public static void WriteLineCyan(string message, params object[] args)
        {
            Write(ConsoleColor.Cyan, string.Format(message, args));
        }

        public static void WriteLineGray(string message, params object[] args)
        {
            Write(ConsoleColor.Gray, string.Format(message, args));
        }

        public static void WriteLineMagenta(string message, params object[] args)
        {
            Write(ConsoleColor.Magenta, string.Format(message, args));
        }

        public static void WriteLineWhite(string message, params object[] args)
        {
            Write(ConsoleColor.White, string.Format(message, args));
        }
                
        private static void Write(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
    }
}
