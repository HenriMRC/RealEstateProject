﻿using System.Net;
using System.Xml.Serialization;

namespace RealEstateProject;

internal class Program
{
    private const string ASSETS_PATH = ".\\Assets";
    private const string SAVE_PATH = ".\\Saves";
    private const string INPUT_NAME = "input.csv";

    private const int SEARCH_SIZE = 110;
    private const int SEARCH_LIMIT = 10_000;

    private const char ARG_KEY_END = ':';
    private const string ARG_KEY_ASSETS_PATH = "-assets-path";
    private const string ARG_KEY_INPUT_FILE = "-input-file";
    private const string ARG_KEY_INPUT_PATH = "-input-path";

    static void Main(string[] args)
    {
        //ZipWriter.SaveJson("{\"test\":0}", ".\\Data\\Test.zip", "Test.json");
        //ZipWriter.SaveJson("{\"test\":1}", ".\\Data\\Test.zip", "TestFolder\\Test.json");
        //return;

        XmlSerializer serializer = new(typeof(Input));
        Input? input;
        using (FileStream reader = File.OpenRead(".\\Assets\\input.xml"))
            input = (Input?)serializer.Deserialize(reader);

        if (input == null)
            throw new ArgumentNullException(nameof(input));

        string date = DateTime.Now.ToString("yyyy_MM_dd");
        foreach (Item item in input.Items)
        {
            foreach (Business business in item.Business)
            {
                Console.WriteLine($"Scrape: {item.State} {item.City} {business.UrlKind}");
                ScrapeListings(item, business.UrlKind, date);
            }
        }

        Console.WriteLine("Finished scrapping. Press [Enter] to close program.");
        Console.ReadLine();
        return;

        FileInfo inputFile = GetInputFile(args);
        if (!inputFile.Exists)
            ThrowCriticalError($"File ({inputFile.FullName}) not found.", 2);

        Console.WriteLine($"Input file: {inputFile.FullName}\n");
        Console.ReadLine();
    }

    private static FileInfo GetInputFile(string[] args)
    {
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

            string argKey = arg.Substring(0, argKeyEndIndex);
            string argValue = arg.Substring(argKeyEndIndex + 1, argKeyEndIndex);

            switch (argKey)
            {
                case ARG_KEY_ASSETS_PATH:
                    if (fileName == null)
                        assetsPath = argValue;
                    else
                        return new(Path.Combine(argValue, fileName));
                    break;
                case ARG_KEY_INPUT_FILE:
                    if (assetsPath == null)
                        fileName = argValue;
                    else
                        return new(Path.Combine(assetsPath, argValue));
                    break;
                case ARG_KEY_INPUT_PATH:
                    return new(argValue);
                default:
                    ThrowCriticalError($"Argument key not supported: {arg}", 160);
                    break;
            }
        }

        if (assetsPath == null && fileName != null)
            ThrowCriticalError($"Argument key [{ARG_KEY_ASSETS_PATH}] must be acompain by [{ARG_KEY_INPUT_FILE}]", 160);

        if (fileName == null && assetsPath != null)
            ThrowCriticalError($"Argument key [{ARG_KEY_INPUT_FILE}] must be acompain by [{ARG_KEY_ASSETS_PATH}]", 160);

        return new(Path.Combine(ASSETS_PATH, INPUT_NAME));
    }

    private static void ThrowCriticalError(string message, int exitCode)
    {
        Console.Error.WriteLine($"Critical error:\n{message}\n\nProcess cannot continue.\nPress ENTER to close.");
        Console.ReadLine();
        Environment.Exit(exitCode);
    }

    private static void ScrapeListings(Item item, UrlKind urlKind, string date)
    {
        UrlBuilder builder = new(item, urlKind);
        UrlKindUtility.GetBusinessAndTypeFromUrlKind(urlKind, out string business, out _, out _, out _);

        HttpClient client = GetHttpClient();

        string zipPath = $"{SAVE_PATH}\\{date}\\{item.City}\\{urlKind}.zip";

        string url;

        Task<HttpResponseMessage> request;
        HttpResponseMessage requestResponse;

        int priceMin = 0;
        int highestPrice = 0;
        int index = 0;
        int fileName = 0;


        using (ResponseProcessor processor = new(zipPath))
        {
            while (true)
            {

                int searchSize = SEARCH_LIMIT - index;
                if (searchSize == 0)
                {
                    if (priceMin == highestPrice)
                        throw new Exception();

                    priceMin = highestPrice;
                    index = 0;
                    searchSize = SEARCH_SIZE;
                }
                else if (searchSize > SEARCH_SIZE)
                    searchSize = SEARCH_SIZE;

                Console.WriteLine($"Progress: {index} | {searchSize} | {priceMin}RS");

                url = builder.GetUrl(index, searchSize, priceMin);
                request = client.GetAsync(url);
                requestResponse = request.Result;

                switch (requestResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Task<string> readTask = requestResponse.Content.ReadAsStringAsync();
                        string json = readTask.Result;
                        processor.Process(json, business, ref highestPrice, out int count);

                        if (count < searchSize)
                            return;

                        index += searchSize;
                        fileName++;
                        break;
                    case HttpStatusCode.TooManyRequests:
                        Console.WriteLine(requestResponse.StatusCode);

                        client = GetHttpClient();
                        break;
                    default:
                        Console.WriteLine($"[{index}:{searchSize}] - {requestResponse.StatusCode}");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }

    private static HttpClient GetHttpClient()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("x-domain", "www.vivareal.com.br");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return client;
    }
}