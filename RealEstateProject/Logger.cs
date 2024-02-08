using System;
using System.Collections.Generic;
using System.Threading;

namespace RealEstateProject;

internal class Logger
{
    private static int s_line;
    private static readonly object s_lineLock = new();

    private readonly int _line;

    private readonly string _description;

    private readonly static Queue<LogItem> s_queue = new();
    private readonly static object s_queueLock = new();

    static Logger()
    {
        s_line = 0;
    }

    private Logger(string description, int line)
    {
        _line = line;
        _description = description + ": ";
    }

    internal void Log(string message)
    {
        lock (s_queueLock)
        {
            s_queue.Enqueue(new(_line, _description + message));
        }
    }

    internal static Logger GetLogger(string description)
    {
        Logger output;
        int line;
        lock (s_lineLock)
        {
            line = s_line;
            s_line++;
        }

        output = new(description, line);

        return output;
    }

    internal static bool Process()
    {
        LogItem? log;
        lock (s_queueLock)
        {
            if (!s_queue.TryDequeue(out log))
                return false;
        }

        if (log == null)
            return false;

        log.Log();
        return true;
    }

    internal static void ProcessLoop()
    {
        while (true)
        {
            while (Process()) { }
            Thread.Sleep(200);
        }
    }
}