using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;

namespace Wallet.Services.Encryption
{
    public class EncryptionService : IEncryptionService
    {
        private readonly SecretClient _secretClient;

        public EncryptionService(string keyVaultUrl)
        {
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        private async Task<byte[]> GetSecretAsync(string name)
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(name);
            return Encoding.UTF8.GetBytes(secret.Value);
        }

        public async Task<string> EncryptAsync(string plainText)
        {
            byte[] key = await GetSecretAsync("encryption-key");
            byte[] iv = await GetSecretAsync("encryption-iv");

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public async Task<string> DecryptAsync(string cipherText)
        {
            byte[] key = await GetSecretAsync("encryption-key");
            byte[] iv = await GetSecretAsync("encryption-iv");

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
