using RealEstateProject.Data;
using System.Xml.Serialization;

namespace RealEstateProject.InputBreaker;

internal class Program
{
    private const string DEFAULT_ASSETS_PATH = "..\\Assets";
    private const string DEFAULT_INPUT_NAME = "input_sp.xml";
    private const int LIMIT = 120;

    static void Main(string[] args)
    {
        FileInfo inputFile = new(Path.Combine(DEFAULT_ASSETS_PATH, DEFAULT_INPUT_NAME));
        if (!inputFile.Exists)
            throw new Exception();

        XmlSerializer serializer = new(typeof(Input));
        Input? input;
        using (FileStream reader = inputFile.OpenRead())
            input = (Input?)serializer.Deserialize(reader);
        if (input == null)
            throw new NullReferenceException(nameof(input));

        string outputFilePrefix = Path.GetFileNameWithoutExtension(inputFile.Name);
        string directoryName = inputFile.Directory?.FullName ?? throw new Exception();

        for (int i = 0; i < input.Items.Length; i += LIMIT)
        {
            int end = i + LIMIT;
            if (end > input.Items.Length)
                end = input.Items.Length;
            Item[] items = input.Items[i..end];
            Input output = new() { Items = items };
            int index = i / LIMIT;
            FileInfo outputFile = new(Path.Combine(directoryName, $"{outputFilePrefix}_{index}{inputFile.Extension}"));
            using (FileStream stream = outputFile.Open(FileMode.Create, FileAccess.Write))
                serializer.Serialize(stream, output);
        }
    }
}