using Microsoft.AspNetCore.Http;
using Wallet.Services.Models;

namespace Wallet.Services.Contracts
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
    }
}
