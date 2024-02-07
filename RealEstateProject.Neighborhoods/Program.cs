﻿using RealEstateProject.Data;
using System.Net;
using System.Text.Json.Nodes;

namespace RealEstateProject.Neighborhoods;

internal class Program
{
    const string CITY = "São Paulo - SP";

    static void Main(string[] args)
    {
        const int SIZE = 110;
        int from = 0;

        UrlKind[] kinds = Enum.GetValues<UrlKind>();
        Dictionary<string, JsonObject> results = [];
        foreach (UrlKind kind in kinds)
        {
            UrlBuilder urlBuilder = new(CITY, kind);

            bool keepGoing;
            do
            {
                using HttpClient client = GetHttpClient();
                keepGoing = Resquest(client, urlBuilder, SIZE, ref from, results);
            }
            while (keepGoing);
        }

        foreach ((string id, _) in results)
        {
            Console.WriteLine(id);
        }

        Console.WriteLine();
        Console.WriteLine($"Finished with {results.Count}");
        Console.ReadLine();
    }

    private static HttpClient GetHttpClient()
    {
        //TODO: HttpClient is IDisposable, should be disposed.
        HttpClient client = new();

        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("X-Deviceid", "undefined");
        client.DefaultRequestHeaders.Add("X-Domain", ".vivareal.com.br");

        return client;
    }

    private static bool Resquest(HttpClient client, UrlBuilder urlBuilder, int size, ref int from, Dictionary<string, JsonObject> results)
    {
        while (true)
        {
            string url = urlBuilder.GetUrl(from, size);
            HttpResponseMessage response = client.GetAsync(url).Result;

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    string content = response.Content.ReadAsStringAsync().Result;
                    ProcessData(content, out int count, results);
                    if (count < size)
                        return false;
                    from += count;
                    break;
                case HttpStatusCode.TooManyRequests:
                    return true;
                default:
                    throw new Exception(response.StatusCode.ToString());
            }
        }
    }

    private static void ProcessData(string content, out int count, Dictionary<string, JsonObject> results)
    {
        JsonNode? node = JsonNode.Parse(content);
        if (node == null)
            throw new NullReferenceException(nameof(node));

        JsonNode neighborhood = NavigateJsonNode(node, "neighborhood");
        JsonNode result = NavigateJsonNode(neighborhood, "result");
        JsonNode locationsNode = NavigateJsonNode(result, "locations");

        if (locationsNode is not JsonArray locations)
            throw new Exception();

        count = locations.Count;

        foreach (JsonNode? location in locations)
        {
            if (location is null)
                throw new NullReferenceException(nameof(location));

            JsonNode address = NavigateJsonNode(location, "address");
            JsonNode locationId = NavigateJsonNode(address, "locationId");
            string id = locationId.GetValue<string>();
            if (!results.TryAdd(id, (JsonObject)address))
            {
                string one = address.ToJsonString();
                string two = results[id].ToJsonString();
                if (one != two)
                    throw new Exception();
            }
        }
    }

    private static JsonNode NavigateJsonNode(JsonNode node, string property)
    {
        if (node is not JsonObject jObject)
            throw new Exception();

        if (!jObject.TryGetPropertyValue(property, out JsonNode? result) || result == null)
            throw new KeyNotFoundException(property);
        else
            return result;
    }
}