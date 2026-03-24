using Org.BouncyCastle.Bcpg.OpenPgp;
using PgpCore;
using System.Text;
using System.Text.Json;

namespace OnlineRegistration.Server.Services.Interfaces
{
    public interface IPgpService
    {
        Task<string> EncryptAsync(string plainText);
        Task<byte[]> EncryptDataPrepBundleAsync(object payload);
        Task<string> DecryptAsync(string encryptedArmoredString);
    }

    public class PgpService : IPgpService
    {
        private readonly string _publicKeyPath = Path.Combine(AppContext.BaseDirectory, "Keys", "public.asc");
        private readonly string _privateKeyPath = Path.Combine(AppContext.BaseDirectory, "Keys", "private.asc");
        private readonly string _passphrase = "Pssic123@";

        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return plainText;

            // 1. Create keys object
            EncryptionKeys encryptionKeys = new EncryptionKeys(new FileInfo(_publicKeyPath));

            // 2. Pass keys into the CONSTRUCTOR
            using (PGP pgp = new PGP(encryptionKeys))
            {
                // 3. Now you don't need to pass keys to the method at all
                return await pgp.EncryptArmoredStringAsync(plainText);
            }
        }

        public async Task<byte[]> EncryptDataPrepBundleAsync(object payload)
        {
            string jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            EncryptionKeys encryptionKeys = new EncryptionKeys(new FileInfo(_publicKeyPath));

            using (PGP pgp = new PGP(encryptionKeys))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
                    using (MemoryStream inputStream = new MemoryStream(byteArray))
                    {
                        // No keys needed here either because they are in the constructor
                        await pgp.EncryptAsync(inputStream, outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public async Task<string> DecryptAsync(string encryptedArmoredString)
        {
            if (string.IsNullOrWhiteSpace(encryptedArmoredString)) return null;

            try
            {
                // Clean the string: Postman sometimes adds extra newlines or \r that 
                // can break the packet stream reading.
                string cleanedInput = encryptedArmoredString.Trim();

                EncryptionKeys encryptionKeys = new EncryptionKeys(
                    new FileInfo(_publicKeyPath),
                    new FileInfo(_privateKeyPath),
                    _passphrase);

                using (PGP pgp = new PGP(encryptionKeys))
                {
                    // DecryptArmoredStringAsync is usually reliable, but for Type 20 
                    // packets, the stream-based approach is sometimes more stable 
                    // in older versions of BouncyCastle.
                    return await pgp.DecryptArmoredStringAsync(cleanedInput);
                }
            }
            catch (Exception ex)
            {
                // LOGGING TIP: If you still see Packet 20 here, it means 
                // Kleopatra is STILL using AEAD. 
                throw new Exception($"PGP Decryption failed: {ex.Message}");
            }
        }

    }
}