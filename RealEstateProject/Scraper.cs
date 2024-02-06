using System.Net;
using RealEstateProject.XML;

namespace RealEstateProject;

internal class Scraper
{
    private const int SEARCH_SIZE = 110;
    private const int SEARCH_LIMIT = 10_000;

    private readonly Item _inputItem;
    private readonly Logger _logger;

    internal Scraper(Item inputItem)
    {
        _inputItem = inputItem;
        _logger = Logger.GetLogger($"{inputItem.State} {inputItem.City}");
        _logger.Log("idle");
    }

    internal void Scrape(string savesPath, string date)
    {
        _logger.Log("init");
        using ResponseProcessor processor = new($"{savesPath}\\{date}\\{_inputItem.City}.zip");
        foreach (Business business in _inputItem.Business)
        {
            Scrape(_inputItem, business.UrlKind, processor, _logger);
        }
    }

    private static void Scrape(Item item, UrlKind urlKind, ResponseProcessor processor, Logger logger)
    {
        UrlBuilder builder = new(item, urlKind);
        UrlKindUtility.GetBusinessAndTypeFromUrlKind(urlKind, out string business, out _, out _, out _);

        HttpClient client = GetHttpClient();

        string url;

        Task<HttpResponseMessage> request;
        HttpResponseMessage requestResponse;

        long priceMin = 0;
        long highestPrice = 0;
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

            logger.Log($"{urlKind.ToString()[..3]} | {index:0000} | {searchSize:000} | R${priceMin}");

            url = builder.GetUrl(index, searchSize, priceMin);
            request = client.GetAsync(url);
            requestResponse = request.Result;

            switch (requestResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    Task<string> readTask = requestResponse.Content.ReadAsStringAsync();
                    string json = readTask.Result;
                    processor.Process(new($"{urlKind}\\{fileName:00000}.json", json), business, ref highestPrice, out int count);

                    if (count < searchSize)
                    {
                        logger.Log("ended");
                        return;
                    }

                    index += searchSize;
                    fileName++;
                    break;
                case HttpStatusCode.TooManyRequests:
                    //Console.WriteLine(requestResponse.StatusCode);
                    client = GetHttpClient();
                    break;
                default:
                    logger.Log($"[{index}:{searchSize}] - {requestResponse.StatusCode}");
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
