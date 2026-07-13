using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TodoListBackend.Services
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(IFormFile file);
    }
}
