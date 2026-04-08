using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace coderush.Services
{
    public interface IFileParserService
    {
        bool IsSupported(string fileName);
        Task<string> ExtractTextAsync(IFormFile file);
    }
}
