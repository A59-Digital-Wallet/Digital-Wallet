using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Models;

namespace Wallet.Services.Contracts
{
    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file);
    }
}
