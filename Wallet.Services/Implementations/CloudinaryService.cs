using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Wallet.Common.Helpers;
using Wallet.Services.Contracts;
using Wallet.Services.Models;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryHelper _cloudinaryHelper; 

    public CloudinaryService(CloudinaryHelper cloudinaryHelper) 
    {
        _cloudinaryHelper = cloudinaryHelper;
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file)
    {
        if (file.Length > 0)
        {
            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Crop("fill").Gravity("face").Width(150).Height(150)
                };

                // Use the helper class to upload the image
                var uploadResult = await _cloudinaryHelper.UploadImageAsync(uploadParams);

                // Map the ImageUploadResult to CloudinaryUploadResult
                return new CloudinaryUploadResult
                {
                    Url = uploadResult.SecureUrl?.AbsoluteUri,
                    PublicId = uploadResult.PublicId
                };
            }
        }

        return null;
    }
}
