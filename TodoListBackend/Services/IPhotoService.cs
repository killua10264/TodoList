using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TodoListBackend.Services
{
    public interface IPhotoService
    {
        Task<PhotoUploadResult> UploadPhotoAsync(IFormFile file);
        Task<bool> DeletePhotoAsync(string publicId);
    }
}
