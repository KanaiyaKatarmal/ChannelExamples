using System.Globalization;
using ChannelsExample.Entity;
using CsvHelper;

namespace ChannelsExample.Worker
{
    public class FileImportWorker : IFileImportWorker
    {
        private readonly ApplicationDbContext _dbContext;

        public FileImportWorker(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Import(FileImportRequest request)
        {
            if (request.FileData == null || request.FileData.Length == 0)
                throw new ArgumentException("File data is empty", nameof(request.FileData));

            request.FileData.Position = 0; // Reset stream position

            using var reader = new StreamReader(request.FileData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var products = csv.GetRecords<Product>().ToList();

            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();
        }
    }
}

