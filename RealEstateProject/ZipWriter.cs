using System.IO.Compression;

namespace RealEstateProject
{
    internal class ZipWriter
    {
        public static void SaveJson(string json, string zipPath, string fileName)
        {
            FileInfo fileInfo = new(zipPath);
            EnsurePath(fileInfo.Directory);

            ZipArchive? archive = null;
            bool loop = true;
            while (loop)
            {
                try
                {
                    archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

                    ZipArchiveEntry? output = archive.GetEntry(fileName);
                    output?.Delete();
                    output = archive.CreateEntry(fileName);

                    using (Stream stream = output.Open())
                    {
                        using (StreamWriter writer = new(stream))
                        {
                            writer.Write(json);
                        }
                    }

                    loop = false;
                }
                catch (IOException exception)
                {
                    Console.WriteLine(exception.Message);
                    const int SLEEP = 1_000;
                    Console.WriteLine($"Sleep {SLEEP}ms");
                    Thread.Sleep(SLEEP);
                }
                finally
                {
                    archive?.Dispose();
                }
            }
        }

        private static void EnsurePath(DirectoryInfo? directory)
        {
            if (directory == null)
                throw new NullReferenceException();

            if (!directory.Exists)
            {
                EnsurePath(directory.Parent);
                directory.Create();
            }
        }
    }
}
