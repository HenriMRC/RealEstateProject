using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using RealEstateProject.Data;

namespace RealEstateProject;

internal class Program
{
    private const string DEFAULT_ASSETS_PATH = "..\\Assets";
    private const string DEFAULT_SAVE_PATH = "..\\Assets\\Saves";
    private const string DEFAULT_INPUT_NAME = "input.xml";

    private const char ARG_KEY_END = ':';
    private const string ARG_KEY_ASSETS_PATH = "-assets-path";
    private const string ARG_KEY_INPUT_FILE = "-input-file";
    private const string ARG_KEY_INPUT_PATH = "-input-path";
    private const string ARG_KEY_SAVES_PATH = "-saves-path";

    private const int THREADS_MIN = 2; //Main and logger
    private const int THREADS_DEFAULT = 4;

    private static Queue<Scraper>? _scrapers;
    private static readonly object _scrapersLock = new();

    private static string? _savesPath;
    private static string? _date;

    private static readonly Thread s_loggerThread;
    private static Thread[] s_scrapersThread;

    static Program()
    {
        s_loggerThread = new(Logger.ProcessLoop);
        s_scrapersThread = Array.Empty<Thread>();
    }

    static void Main(string[] args)
    {
        s_loggerThread.Start();

        XmlSerializer serializer = new(typeof(Input));
        Input? input;

        GetInputFile(args, out FileInfo inputFile, out _savesPath);
        using (FileStream reader = inputFile.OpenRead())
            input = (Input?)serializer.Deserialize(reader);

        if (input == null)
            throw new NullReferenceException(nameof(input));
        _date = DateTime.Now.ToString("yyyy_MM_dd");

        lock (_scrapersLock)
        {
            _scrapers = new(input.Items.Length);
            foreach (Item item in input.Items)
                _scrapers.Enqueue(new(item));
        }

        int extraThreadCount = THREADS_DEFAULT - THREADS_MIN;
        if (extraThreadCount > _scrapers.Count - 1)
            extraThreadCount = _scrapers.Count - 1;

        s_scrapersThread = new Thread[extraThreadCount];
        for (int i = 0; i < s_scrapersThread.Length; i++)
        {
            s_scrapersThread[i] = new(ScrapeLoop);
            s_scrapersThread[i].Start();
        }

        ScrapeLoop();

        while (s_scrapersThread.Any(t => t.ThreadState == ThreadState.Running))
        {
            Thread.Sleep(200);
        }

        Console.WriteLine("Finished scrapping. Press [Enter] to close program.");
        Console.Beep();
        Console.ReadLine();
    }

    private static void GetInputFile(string[] args, out FileInfo inputFile, out string savesPath)
    {
        inputFile = null!;
        savesPath = null!;

        string? assetsPath = null;
        string? fileName = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            int argKeyEndIndex = arg.IndexOf(ARG_KEY_END);
            if (argKeyEndIndex == -1)
                ThrowCriticalError($"Argument not supported: {arg}", 160);
            if (argKeyEndIndex == 0)
                ThrowCriticalError($"Argument must contain a key: {arg}", 160);
            if (argKeyEndIndex + 1 == arg.Length)
                ThrowCriticalError($"Argument must contain a value: {arg}", 160);

            string argKey = arg[..argKeyEndIndex];
            string argValue = arg.Substring(argKeyEndIndex + 1, argKeyEndIndex);

            switch (argKey)
            {
                case ARG_KEY_ASSETS_PATH:
                    assetsPath = argValue;
                    break;
                case ARG_KEY_INPUT_FILE:
                    fileName = argValue;
                    break;
                case ARG_KEY_INPUT_PATH:
                    inputFile = new(argValue);
                    break;
                case ARG_KEY_SAVES_PATH:
                    savesPath = argValue;
                    break;
                default:
                    ThrowCriticalError($"Argument key not supported: {arg}", 160);
                    break;
            }
        }

        if (inputFile == null)
        {
            if (assetsPath == null)
            {
                if (fileName == null)
                    inputFile = new(Path.Combine(DEFAULT_ASSETS_PATH, DEFAULT_INPUT_NAME));
                else
                    inputFile = new(Path.Combine(DEFAULT_ASSETS_PATH, fileName));
            }
            else
            {
                if (fileName == null)
                    inputFile = new(Path.Combine(assetsPath, DEFAULT_INPUT_NAME));
                else
                    inputFile = new(Path.Combine(assetsPath, fileName));
            }
        }
        savesPath ??= DEFAULT_SAVE_PATH;
    }

    private static void ThrowCriticalError(string message, int exitCode)
    {
        Console.Error.WriteLine($"Critical error:\n{message}\n\nProcess cannot continue.\nPress ENTER to close.");
        Console.ReadLine();
        Environment.Exit(exitCode);
    }

    private static void ScrapeLoop()
    {
        if (_scrapers == null)
            throw new NullReferenceException(nameof(_scrapers));

        if (_savesPath == null)
            throw new NullReferenceException(nameof(_savesPath));
        
        if (_date == null)
            throw new NullReferenceException(nameof(_date));

        Scraper? item;
        while (true)
        {
            lock (_scrapersLock)
            {
                if (!_scrapers.TryDequeue(out item))
                    return;
            }
            item.Scrape(_savesPath, _date);
        }
    }
}