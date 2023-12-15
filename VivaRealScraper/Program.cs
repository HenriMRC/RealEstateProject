using HtmlAgilityPack;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Text.Json;

namespace VivaRealScraper
{
    internal class Program
    {
        private const string ASSETS_PATH = ".\\Assets";
        private const string INPUT_NAME = "input.csv";

        private const string CLASS_1 = "results-summary__count";
        private const string CLASS_2 = "js-total-records";

        private const int SEARCH_SIZE = 110;
        private const int SEARCH_LIMIT = 10_000;

        private const char ARG_KEY_END = ':';
        private const string ARG_KEY_ASSETS_PATH = "-assets-path";
        private const string ARG_KEY_INPUT_FILE = "-input-file";
        private const string ARG_KEY_INPUT_PATH = "-input-path";

        private const string URL_SEARCH = "https://www.vivareal.com.br/venda/santa-catarina/florianopolis/#onde=Brasil,Santa%20Catarina,Florian%C3%B3polis,,,,,,BR%3ESanta%20Catarina%3ENULL%3EFlorianopolis,,,";
        private const string MIN_PRICE = "&preco-desde=";

        private const string URL_1 = "https://glue-api.vivareal.com/v2/listings?addressCity=Florian%C3%B3polis&addressLocationId=BR%3ESanta%20Catarina%3ENULL%3EFlorianopolis&addressNeighborhood=&addressState=Santa%20Catarina&addressCountry=Brasil&addressStreet=&addressZone=&addressPointLat=-27.594804&addressPointLon=-48.556929&business=SALE&facets=amenities&unitTypes=&unitSubTypes=&unitTypesV3=&usageTypes=&";
        //nothing here
        private const string URL_OP_1 = "priceMin=";
        //min prize
        private const string URL_OP_2 = "&";
        private const string URL_2 = "listingType=USED&parentId=null&categoryPage=RESULT&images=webp&includeFields=search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount)%2Cpage%2CseasonalCampaigns%2CfullUriFragments%2Cnearby(search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount))%2Cexpansion(search(result(listings(listing(displayAddressType%2Camenities%2CusableAreas%2CconstructionStatus%2ClistingType%2Cdescription%2Ctitle%2CunitTypes%2CnonActivationReason%2CpropertyType%2CunitSubTypes%2Cid%2Cportal%2CparkingSpaces%2Caddress%2Csuites%2CpublicationType%2CexternalId%2Cbathrooms%2CusageTypes%2CtotalAreas%2CadvertiserId%2Cbedrooms%2CpricingInfos%2CshowPrice%2Cstatus%2CadvertiserContact%2CvideoTourLink%2CwhatsappNumber%2Cstamps)%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier)%2Cmedias%2CaccountLink%2Clink))%2CtotalCount))%2Caccount(id%2Cname%2ClogoUrl%2ClicenseNumber%2CshowAddress%2ClegacyVivarealId%2Cphones%2Ctier%2Cphones)%2Cfacets&size=";
        //search size
        private const string URL_3 = "&from=";
        //search start
        private const string URL_4 = "&sort=pricingInfos.price%20ASC%20sortFilter%3ApricingInfos.businessType%3D%27SALE%27&q=&developmentsSize=";
        //development (building in construction) search size
        private const string URL_5 = "&__vt=B&levels=CITY&ref=&pointRadius=&isPOIQuery=";

        static void Main(string[] args)
        {
            string today = DateTime.Now.ToString("yyyy_MM_dd");

            FileInfo inputFile = GetInputFile(args);
            if (!inputFile.Exists)
                ThrowCriticalError($"File ({inputFile.FullName}) not found.", 2);

            Console.WriteLine($"Input file: {inputFile.FullName}\n");

            ScrapeListings("Buy", "FLorianópolis", "SC", URL_SEARCH);

            Console.WriteLine(today);
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

        private static IReadOnlyList<IReadOnlyDictionary<string, string>> ParseCSVWithHeader(FileInfo file)
        {
            List<IReadOnlyDictionary<string, string>> output = new();
            using (TextFieldParser parser = new(file.FullName))
            {
                parser.SetDelimiters(",");

                if (!parser.EndOfData)
                {
                    string[]? headers = parser.ReadFields();
                    if (headers == null)
                        throw new ArgumentNullException(nameof(headers)); //TODO: review this

                    while (!parser.EndOfData)
                    {
                        string[]? values = parser.ReadFields();
                        if (values == null || values.Length == 0)
                            continue;

                        if (values.Length > headers.Length)
                            throw new Exception(); //TODO: review this

                        Dictionary<string, string> row = new();
                        for (int i = 0; i < values.Length; i++)
                            row.Add(headers[i], values[i]);
                        output.Add(row);
                    }
                }
            }

            return output.ToArray();
        }

        private static void ScrapeListings(string type, string city, string state, string URL_search)
        {
            Console.WriteLine($"Scrape: {state} {city} {type}");

            HttpClient client = new();

            HtmlDocument doc = new();

            Task<HttpResponseMessage> request = client.GetAsync(URL_search);
            HttpResponseMessage requestResponse = request.Result;

            int listingsCount = 0;

            switch (requestResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                    Task<string> readTask = requestResponse.Content.ReadAsStringAsync();
                    string html = readTask.Result;

                    doc.LoadHtml(html);
                    HtmlNode node = doc.DocumentNode.Descendants().Single(n => n.HasClass(CLASS_1) && n.HasClass(CLASS_2));

                    listingsCount = int.Parse(node.InnerHtml.Trim().Replace(".", ""));
                    break;
                default:
                    Console.WriteLine(requestResponse.StatusCode);
                    break;
            }

            client = new();
            client.DefaultRequestHeaders.Add("x-domain", "www.vivareal.com.br");
            Console.WriteLine($"Listing count: {listingsCount}");
            int priceMin = 0;
            int highestPrice = 0;
            int totalCount = 0;
            for (int i = 0; i < listingsCount;)
            {
                int searchSize = SEARCH_LIMIT - i;
                if (searchSize == 0)
                {
                    priceMin = highestPrice;
                    i = 0;
                    searchSize = SEARCH_SIZE;
                }
                else if (searchSize > SEARCH_SIZE)
                    searchSize = SEARCH_SIZE;

                Console.WriteLine($"Progress: {i} | {searchSize} | {priceMin}RS");
                string pageIndex = i == 0 ? string.Empty : i.ToString();

                //string url = $"{URL_request_1}{searchSize}{URL_request_2}{pageIndex}{URL_request_3}";

                string url = $"{URL_1}";
                if (priceMin != 0)
                    url += $"{URL_OP_1}{priceMin}{URL_OP_2}";
                url += $"{URL_2}{searchSize}{URL_3}{pageIndex}{URL_4}{searchSize}{URL_5}";

                request = client.GetAsync(url);
                requestResponse = request.Result;

                switch (requestResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Task<string> readTask = requestResponse.Content.ReadAsStringAsync();
                        string json = readTask.Result;
                        UpdateHighestPrize(json, ref highestPrice, out int count);
                        //TODO: save

                        totalCount += count;
                        if (count < searchSize)
                        {
                            Console.WriteLine($"Total count: {totalCount}");
                            return;
                        }

                        i += searchSize;
                        break;
                    case HttpStatusCode.TooManyRequests:
                        readTask = requestResponse.Content.ReadAsStringAsync();
                        json = readTask.Result;

                        Console.WriteLine(requestResponse.StatusCode);

                        client = new();
                        client.DefaultRequestHeaders.Add("x-domain", "www.vivareal.com.br");

                        //                        Console.ReadLine();
                        break;
                    default:
                        readTask = requestResponse.Content.ReadAsStringAsync();
                        json = readTask.Result;

                        Console.WriteLine($"[{pageIndex}:{searchSize}] - {requestResponse.StatusCode}");
                        Console.ReadLine();
                        break;
                }

                //Thread.Sleep(1_000); //TODO: review - last iteration does no need
            }

            Console.WriteLine($"Progress: 100.00%");
        }

        private static void UpdateHighestPrize(string json, ref int highestPrize, out int responseCount)
        {
            //TODO:replace JsonDocument with JsonNode (???) or maybe JsonObject
            //JsonNode? test = JsonNode.Parse(json);
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                int current = Step1(doc.RootElement, out responseCount);
                if (current > highestPrize)
                    highestPrize = current;
            }

        }

        private static int Step1(JsonElement element, out int count)
        {
            int output = int.MinValue;
            count = 0;

            if (element.TryGetProperty("search", out JsonElement child))
            {
                int current = Step2(child, out count);
                if (output < current)
                    output = current;
            }

            if (element.TryGetProperty("developments", out child))
            {
                int current = Step1(child, out count);
                if (output < current)
                    output = current;
            }

            return output;
        }

        private static int Step2(JsonElement element, out int count)
        {
            if (element.TryGetProperty("result", out JsonElement child))
                return Step3(child, out count);

            throw new Exception();
        }

        private static int Step3(JsonElement element, out int count)
        {
            if (element.TryGetProperty("listings", out JsonElement child))
                return Step4(child, out count);

            throw new Exception();
        }

        private static int Step4(JsonElement element, out int count)
        {
            int output = int.MinValue;

            int length = element.GetArrayLength();
            for (int i = 0; i < length; i++)
            {
                int current = Step5(element[i]);
                if (output < current)
                    output = current;
            }

            count = length;
            return output;
        }

        private static int Step5(JsonElement element)
        {
            if (element.TryGetProperty("listing", out JsonElement child))
                return Step6(child);

            throw new Exception();
        }

        private static int Step6(JsonElement element)
        {
            if (element.TryGetProperty("pricingInfos", out JsonElement child))
                return Step7(child);

            throw new Exception();
        }

        private static int Step7(JsonElement element)
        {
            //TODO
            //if (element.GetArrayLength() != 1)
            //    throw new ArgumentOutOfRangeException();

            int length = element.GetArrayLength();
            for (int i = 0; i < length; i++)
                return Step8(element[i]);

            throw new Exception();
        }

        private static int Step8(JsonElement element)
        {
            if (element.TryGetProperty("price", out JsonElement child))
                return int.Parse(child.GetString()!);

            throw new Exception();
        }
    }
}