using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadDirectory;

        public LocalFileStorageService(IConfiguration configuration)
        {
            var settingsDir = configuration["DocumentSettings:UploadDirectory"] ?? "AppData/uploads";
            _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), settingsDir);
            
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
            }
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var fullPath = Path.Combine(_uploadDirectory, fileName);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var destinationStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fileStream.CopyToAsync(destinationStream);
            }

            return fileName;
        }

        public Task<Stream> DownloadAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadDirectory, filePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("El archivo solicitado no existe.", fullPath);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadDirectory, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.CompletedTask;
        }
    }
}
