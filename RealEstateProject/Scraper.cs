using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RealEstateProject.Data;

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
        string description = $"{inputItem.State} {inputItem.City}";
        if (!string.IsNullOrWhiteSpace(inputItem.Neighborhood))
            description += $" {inputItem.Neighborhood}";
        _logger = Logger.GetLogger(description);
        _logger.Log("idle");
    }

    internal void Scrape(string savesPath, string date)
    {
        try
        {
            _logger.Log("init");

            savesPath += $"\\{date}\\{_inputItem.City}";
            if (!string.IsNullOrWhiteSpace(_inputItem.Zone))
                savesPath += $"\\{_inputItem.Zone}";
            if (!string.IsNullOrWhiteSpace(_inputItem.Neighborhood))
                savesPath += $"\\{_inputItem.Neighborhood}";
            savesPath += ".zip";

            using ResponseProcessor processor = new(savesPath);
            foreach (Business business in _inputItem.Business)
                Scrape(_inputItem, business.UrlKind, processor, _logger);

            _logger.Log("finished");
        }
        catch (Exception e)
        {
            _logger.Log(e.Message);
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
                    string json;
                    int count;
                    using (Task<string> readTask = requestResponse.Content.ReadAsStringAsync())
                    {
                        json = readTask.Result;
                        processor.Process(new($"{urlKind}\\{fileName:00000}.json", json), business, ref highestPrice, out count);
                    }

                    if (count < searchSize)
                    {
                        logger.Log("writing");
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
        //TODO: HttpClient is IDisposable, should be disposed.
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("x-domain", "www.vivareal.com.br");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return client;
    }
}
