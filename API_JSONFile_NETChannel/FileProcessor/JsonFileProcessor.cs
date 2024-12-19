using API_JSONFile_NETChannel.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

        private async Task<List<ProductInfo>> ReadJsonFileAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var products = await JsonSerializer.DeserializeAsync<List<ProductInfo>>(stream);
            return products ?? new List<ProductInfo>();
        }

        private async Task InsertProductsIntoDatabaseAsync(List<ProductInfo> products)
        {
            Task.Delay(7000).Wait();
            await ctx.ProductInfos.AddRangeAsync(products);
            await ctx.SaveChangesAsync();
        }

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
