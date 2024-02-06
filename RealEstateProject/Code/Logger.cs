namespace RealEstateProject;

internal class Logger
{
    private const int LINE_WIDTH = 200;
    private static int s_line;
    private static readonly object s_lineLock = new();

    private readonly int _line;
    private readonly int _lineStart;
    private readonly int _lineMaxLength;

    private readonly static Queue<LogItem> s_queue = new();
    private readonly static object s_queueLock = new();

    static Logger()
    {
        s_line = 0;
#pragma warning disable CA1416 
        Console.WindowWidth = LINE_WIDTH;
#pragma warning restore CA1416 
    }

    private Logger(string description, int line)
    {
        _line = line;
        _lineStart = description.Length;
        _lineMaxLength = LINE_WIDTH - _lineStart;
    }

    internal void Log(string message)
    {
        lock (s_queueLock)
        {
            s_queue.Enqueue(new(_line, _lineStart, _lineMaxLength, message));
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

        description += ": ";
        output = new(description, line);
        lock (s_queueLock)
        {
            s_queue.Enqueue(new(line, 0, LINE_WIDTH, description));
        }

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