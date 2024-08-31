using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Common.Helpers
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryHelper(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public virtual async Task<ImageUploadResult> UploadImageAsync(ImageUploadParams uploadParams)
        {
            return await _cloudinary.UploadAsync(uploadParams);
        }
    }
}
