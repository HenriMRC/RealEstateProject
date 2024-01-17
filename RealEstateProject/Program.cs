using System.Net;
using System.Xml.Serialization;

namespace RealEstateProject;

internal class Program
{
    private const string DEFAULT_ASSETS_PATH = ".\\Assets";
    private const string DEFAULT_SAVE_PATH = ".\\Assets\\Saves";
    private const string DEFAULT_INPUT_NAME = "input.xml";

    private const int SEARCH_SIZE = 110;
    private const int SEARCH_LIMIT = 10_000;

    private const char ARG_KEY_END = ':';
    private const string ARG_KEY_ASSETS_PATH = "-assets-path";
    private const string ARG_KEY_INPUT_FILE = "-input-file";
    private const string ARG_KEY_INPUT_PATH = "-input-path";
    private const string ARG_KEY_SAVES_PATH = "-saves-path";

    static void Main(string[] args)
    {
        XmlSerializer serializer = new(typeof(Input));
        Input? input;

        GetInputFile(args, out FileInfo inputFile, out string savesPath);
        using (FileStream reader = inputFile.OpenRead())
            input = (Input?)serializer.Deserialize(reader);

        if (input == null)
            throw new ArgumentNullException(nameof(input));

        string date = DateTime.Now.ToString("yyyy_MM_dd");
        using (ResponseProcessor processor = new($"{savesPath}\\{date}.zip"))
        {
            foreach (Item item in input.Items)
            {
                foreach (Business business in item.Business)
                {
                    Console.WriteLine($"Scrape: {item.State} {item.City} {business.UrlKind}");
                    ScrapeListings(item, business.UrlKind, processor);
                }
            }
        }

        Console.WriteLine("Finished scrapping. Press [Enter] to close program.");
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

            string argKey = arg.Substring(0, argKeyEndIndex);
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

    private static void ScrapeListings(Item item, UrlKind urlKind, ResponseProcessor processor)
    {
        UrlBuilder builder = new(item, urlKind);
        UrlKindUtility.GetBusinessAndTypeFromUrlKind(urlKind, out string business, out _, out _, out _);

        HttpClient client = GetHttpClient();

        string url;

        Task<HttpResponseMessage> request;
        HttpResponseMessage requestResponse;

        int priceMin = 0;
        int highestPrice = 0;
        int index = 0;
        int fileName = 0;

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
                    processor.Process(new($"{item.City}\\{urlKind}\\{fileName:00000}.json", json), business, ref highestPrice, out int count);

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

    private static HttpClient GetHttpClient()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("x-domain", "www.vivareal.com.br");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return client;
    }
}