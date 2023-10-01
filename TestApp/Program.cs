using SevenZip;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = @"C:\Users\gavet\Documents\Test\Archives\temp_p.7z";
            var password = "";
            try
            {
                var arch = new SevenZipExtractor(File.OpenRead(filePath), password);
                arch.ArchiveProperties.ToList().ForEach(p => Console.WriteLine($"{p.Name}: {p.Value}"));
                Console.WriteLine($"Contains: {arch.ArchiveFileData.Count} files");
                var enc = arch.ArchiveFileData.Any(file => file.Encrypted || file.Method.Contains("Crypto") || file.Method.Contains("AES"));
                Console.WriteLine($"Encrypted: {enc}, {arch.ArchiveProperties.FirstOrDefault(x => x.Name == "Encrypted").Value}");
                arch.ExtractFile("orig\\dxwebsetup.exe", new MemoryStream());
            }
            catch(SevenZipOpenFailedException ex)
            {
                Console.WriteLine($"Error: {ex.Result}");
            }
            catch (ExtractionFailedException ex)
            {
                Console.WriteLine($"Error: {ex.Result}");
            }

            /*var compressor = new SevenZipCompressor()
            {
                ArchiveFormat = OutArchiveFormat.Zip
            };
            compressor.Compressing += Compressor_Compressing;
            compressor.CompressDirectory(@"C:\Users\gavet\Documents\Test", @"C:\Users\gavet\Documents\Test.zip");*/
        }

        private static void Compressor_Compressing(object? sender, ProgressEventArgs e)
        {
            Console.WriteLine(e.BytesProcessed / (double)e.BytesCount * 100);
        }
    }
}