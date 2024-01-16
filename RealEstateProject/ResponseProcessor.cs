﻿using System.IO.Compression;
using System.Text.Json;

namespace RealEstateProject;

internal class ResponseProcessor : IDisposable
{
    private ZipArchive _zipArchive;
    private Task _task;
    private bool _finish = false;

    private Queue<string> _queue = new();
    private object _queueLock = new();

    internal ResponseProcessor(string zipPath)
    {
        FileInfo fileInfo = new(zipPath);
        fileInfo.Directory?.Create();

        _zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update);
        _task = Task.Run(MainLoop);
    }

    private void MainLoop()
    {
        while (true)
        {
            string json;
            lock (_queueLock)
            {
                if (_finish && _queue.Count == 0)
                    return;

                if (!_queue.TryDequeue(out json!))
                    continue;
            }

            string fileName = $"{_zipArchive.Entries.Count:00000}.json";
            ZipArchiveEntry? output = _zipArchive.GetEntry(fileName);
            output?.Delete();
            output = _zipArchive.CreateEntry(fileName, CompressionLevel.SmallestSize);

            using (Stream stream = output.Open())
            {
                using (StreamWriter writer = new(stream))
                {
                    writer.WriteLine(json);
                }
            }
        }
    }

    internal void Process(string json, string business, ref int highestPrize, out int responseCount)
    {
        lock (_queueLock)
        {
            _queue.Enqueue(json);
        }

        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            int current = Step1(doc.RootElement, business, out responseCount);
            if (current > highestPrize)
                highestPrize = current;
        }
    }

    private static int Step1(JsonElement element, string business, out int count)
    {
        int output = int.MinValue;
        count = 0;

        if (element.TryGetProperty("search", out JsonElement child))
        {
            int current = Step2(child, business, out count);
            if (output < current)
                output = current;
        }

        if (element.TryGetProperty("developments", out child))
        {
            int current = Step1(child, business, out count);
            if (output < current)
                output = current;
        }

        return output;
    }

    private static int Step2(JsonElement element, string business, out int count)
    {
        if (element.TryGetProperty("result", out JsonElement child))
            return Step3(child, business, out count);

        throw new Exception();
    }

    private static int Step3(JsonElement element, string business, out int count)
    {
        if (element.TryGetProperty("listings", out JsonElement child))
            return Step4(child, business, out count);

        throw new Exception();
    }

    private static int Step4(JsonElement element, string business, out int count)
    {
        int output = int.MinValue;

        int length = element.GetArrayLength();
        for (int i = 0; i < length; i++)
        {
            int current = Step5(element[i], business);
            if (output < current)
                output = current;
        }

        count = length;
        return output;
    }

    private static int Step5(JsonElement element, string business)
    {
        if (element.TryGetProperty("listing", out JsonElement child))
            return Step6(child, business);

        throw new Exception();
    }

    private static int Step6(JsonElement element, string business)
    {
        if (element.TryGetProperty("pricingInfos", out JsonElement child))
        {
            if (Step7(child, business, out int price))
                return price;
            else
            {
                element.TryGetProperty("showPrice", out JsonElement showPrice);
                if (showPrice.GetBoolean())
                    throw new Exception();
                else
                    return 0;
            }
        }

        throw new Exception();
    }

    private static bool Step7(JsonElement element, string business, out int price)
    {
        bool found = false;
        int length = element.GetArrayLength();
        for (int i = 0; i < length; i++)
        {
            JsonElement child = element[i];

            if (child.TryGetProperty("businessType", out JsonElement businessTypeElement))
            {
                string businessType = businessTypeElement.GetString() ?? throw new ArgumentNullException();
                if (businessType == business)
                {
                    if (found)
                        throw new Exception();
                    else
                        found = true;

                    if (Step8(child, out price))
                        return true;
                }
                else
                    continue;
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        price = 0;
        return false;
    }

    private static bool Step8(JsonElement element, out int price)
    {
        if (element.TryGetProperty("price", out JsonElement child))
        {
            price = int.Parse(child.GetString()!);
            return true;
        }
        else
        {
            price = 0;
            return false;
        }
    }

    public void Dispose()
    {
        _finish = true;
        _task.Wait();
        _task.Dispose();

        _zipArchive.Dispose();
    }
}
