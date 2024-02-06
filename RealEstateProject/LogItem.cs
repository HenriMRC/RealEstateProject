using System;

namespace RealEstateProject;

internal class LogItem
{
    private readonly int _line;
    private readonly int _lineStart;
    private readonly int _lineMaxLength;
    private readonly string _message;

    private readonly object s_lock = new();

    internal LogItem(int line, int lineStart, int lineMaxLength, string message)
    {
        _line = line;
        _lineStart = lineStart;
        _lineMaxLength = lineMaxLength;
        _message = message;
    }

    internal void Log()
    {
        lock (s_lock)
        {
            string message = _message.Trim();

            int max = _lineMaxLength;
            if (max > _message.Length)
                max = _message.Length;
            message = _message[..max];

            Console.SetCursorPosition(_lineStart, _line);
            Console.Write(new string(' ', _lineMaxLength));
            Console.SetCursorPosition(_lineStart, _line);
            Console.WriteLine(message);
        }
    }
}