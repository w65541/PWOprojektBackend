using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1
{
    public class RsaImp
    {
        RSA rsa;
        RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA256;
        string keyPriv, keyPub;
        public RsaImp()
        {
            rsa = RSA.Create();

            keyPriv = File.ReadAllText("./RSAprivKey.txt");

            keyPub = File.ReadAllText("./RSApubKey.txt");

            
        }

        public string GetPublicKey()
        {
            return keyPub;
        }

        public string Encode(string x)
        {
            try
            {
            rsa.ImportFromPem(keyPub);
            var ecrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(x), padding);
            return Convert.ToBase64String(ecrypted);
            }
            catch (Exception)
            {

                return "bad";
            }
            
        }

        public string Decode(string x) 
        {
            try
            {
            rsa.ImportFromPem(keyPriv);
            var decrypted=rsa.Decrypt(Convert.FromBase64String(x), padding);
            return Encoding.UTF8.GetString(decrypted); 
            }
            catch (Exception )
            {

                return "bad";
            }
            
        }
    }
}
