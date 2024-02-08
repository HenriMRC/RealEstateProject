using System;

namespace RealEstateProject;

internal class LogItem
{
    private readonly int _line;
    private readonly string _message;

    private readonly static object s_lock = new();

    internal LogItem(int line, string message)
    {
        _line = line;
        _message = message;
    }

    internal void Log()
    {
        lock (s_lock)
        {
            string message = _message.Trim();

            int max = Console.BufferWidth;
            if (max > _message.Length)
                max = _message.Length;
            message = _message[..max];

            Console.SetCursorPosition(0, _line);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, _line);
            Console.WriteLine(message);
        }
    }
}