using API_JSONFile_NETChannel.Models;
using System.Text.Json;

namespace API_JSONFile_NETChannel.FileProcessor
{
    public class JsonFileProcessor(EcommContext ctx)
    {
        private readonly string _uploadsFolder = "uploads";
        private readonly string _processedFilesFolder = "processedFiles";
        public async Task ProcessFilesAsync(string file)
        {
            var products = await ReadJsonFileAsync(file);
            await InsertProductsIntoDatabaseAsync(products);
            MoveFileToProcessedFolder(file);
        }
        /// <summary>
        /// Read JSON file and Deserialize into the List of ProductInfo class
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<List<ProductInfo>> ReadJsonFileAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var products = await JsonSerializer.DeserializeAsync<List<ProductInfo>>(stream);
            return products ?? new List<ProductInfo>();
        }
        /// <summary>
        /// Insert ProductInfo List into the ProductInfo Table
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        private async Task InsertProductsIntoDatabaseAsync(List<ProductInfo> products)
        {
            Task.Delay(7000).Wait();
            await ctx.ProductInfos.AddRangeAsync(products);
            await ctx.SaveChangesAsync();
        }
        /// <summary>
        /// Once the JSON file's data is inserted into the Table Move the file to 'processedFiles' folder 
        /// </summary>
        /// <param name="filePath"></param>
        private void MoveFileToProcessedFolder(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var destinationPath = Path.Combine(_processedFilesFolder, fileName);
            if (File.Exists(destinationPath))
            {
                var fileExtension = Path.GetExtension(fileName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var counter = 1;

                while (File.Exists(destinationPath))
                {
                    var newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
                    destinationPath = Path.Combine(_processedFilesFolder, newFileName);
                    counter++;
                }
            }
            File.Move(filePath, destinationPath);
        }
    }
}
