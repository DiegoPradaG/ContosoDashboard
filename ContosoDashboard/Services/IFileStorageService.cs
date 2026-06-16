using System.IO;
using System.Threading.Tasks;

namespace ContosoDashboard.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadAsync(string filePath);
        Task DeleteAsync(string filePath);
    }
}
