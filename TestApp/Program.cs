using SevenZip;
using System.Text;

namespace TestApp
{
    internal class Program
    {
        private static void Main()
        {
            SevenZipBase.SetLibraryPath(Path.Combine(AppContext.BaseDirectory, "7z64.dll"));

            var extractionDirectory = Path.Combine(
                Path.GetTempPath(),
                "SevenZipSharp-Aot-" + Guid.NewGuid().ToString("N"));
            try
            {
                using var extractor = new SevenZipExtractor(
                    Path.Combine(AppContext.BaseDirectory, "multiple_files.7z"));
                extractor.ExtractArchive(extractionDirectory);
                Ensure(
                    Directory.EnumerateFiles(extractionDirectory, "*", SearchOption.AllDirectories).Any(),
                    "Existing archive extraction returned no files.");
            }
            finally
            {
                if (Directory.Exists(extractionDirectory))
                {
                    Directory.Delete(extractionDirectory, true);
                }
            }

            var payload = Encoding.UTF8.GetBytes("SevenZipSharp NativeAOT smoke test");
            const string password = "aot-password";
            using var input = new MemoryStream(payload);
            using var archive = new MemoryStream();
            var compressor = new SevenZipCompressor
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMethod = CompressionMethod.Lzma2,
                EncryptHeaders = true
            };
            compressor.CompressStream(input, archive, password);

            archive.Position = 0;
            using (var extractor = new SevenZipExtractor(archive, password, true))
            {
                using var extracted = new MemoryStream();
                extractor.ExtractFile(0, extracted);
                Ensure(extracted.ToArray().SequenceEqual(payload), "Compression round trip changed the payload.");
            }

            Console.WriteLine("NativeAOT smoke test passed.");
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
