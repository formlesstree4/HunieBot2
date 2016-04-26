using HunieBot.Host.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace HunieBot.Host.Injection.Implementations
{

    /// <summary>
    ///     An implementation of <see cref="ILogging"/> that is to be used during DEBUG builds of HunieBot2.
    /// </summary>
    internal sealed class DebugLogger : ILogging
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Debug(string message)
        {
            WriteToConsole(ConsoleColor.White, message);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Debug(string format, params object[] content)
        {
            WriteToConsole(ConsoleColor.White, string.Format(format, content));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Fatal(string message, Exception error)
        {
            WriteToConsole(ConsoleColor.Red, $"{message}\r\n{error}");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Fatal(string format, Exception error, params object[] content)
        {
            WriteToConsole(ConsoleColor.Red, string.Format(format, content));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Info(string message)
        {
            WriteToConsole(ConsoleColor.Gray, message);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Info(string format, params object[] content)
        {
            WriteToConsole(ConsoleColor.Gray, string.Format(format, content));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Trace(string message)
        {
            WriteToConsole(ConsoleColor.DarkGray, message);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Trace(string format, params object[] content)
        {
            WriteToConsole(ConsoleColor.DarkGray, string.Format(format, content));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WriteToConsole(ConsoleColor textColor, string message)
        {
            var oldcolor = Console.ForegroundColor;
            Console.ForegroundColor = textColor;
            Console.WriteLine(message);
            Console.ForegroundColor = oldcolor;
        }

    }

}
