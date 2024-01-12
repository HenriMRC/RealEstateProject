using System.IO.Compression;
using System.IO;

namespace VivaRealScraper
{
    internal class ZipWriter
    {
        public static void SaveJson(string json, string zipPath, string fileName)
        {
            FileInfo fileInfo = new(zipPath);
            EnsurePath(fileInfo.Directory);

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                var output = archive.GetEntry(fileName);
                output?.Delete();
                output = archive.CreateEntry(fileName);

                using (Stream stream = output.Open())
                {
                    using (StreamWriter writer = new(stream))
                    {
                        writer.Write(json);
                    }
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
