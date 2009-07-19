using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Rensoft.Crypto
{
    public class RsEncryptor
    {
        private string secret;
        private const string salt = "p C)m<7[Q]ZfAS_Mk`J9";

        public RsEncryptor(string secret)
        {
            this.secret = secret;
        }

        public byte[] Encrypt(string plainText)
        {            
            ICryptoTransform encryptor = getAes().CreateEncryptor();
            MemoryStream stream = new MemoryStream();
            CryptoStream crypto = new CryptoStream(
                stream, 
                encryptor, 
                CryptoStreamMode.Write);

            byte[] utfdata = UTF8Encoding.UTF8.GetBytes(plainText);
            crypto.Write(utfdata, 0, utfdata.Length);
            crypto.Flush();
            crypto.Close();

            return stream.ToArray();
        }

        public string Decrypt(byte[] encryptBytes)
        {
            ICryptoTransform decryptor = getAes().CreateDecryptor();
            MemoryStream stream = new MemoryStream();

            CryptoStream crypto = new CryptoStream(
                stream,
                decryptor, 
                CryptoStreamMode.Write);

            crypto.Write(encryptBytes, 0, encryptBytes.Length);
            crypto.Flush();
            crypto.Close();

            byte[] decryptBytes = stream.ToArray();
            return UTF8Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
        }

        private AesManaged getAes()
        {
            AesManaged aes = new AesManaged();

            // For salt, use secret to make implementation easier.
            Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(
                secret, Encoding.Unicode.GetBytes(salt));

            aes.Key = derive.GetBytes(aes.KeySize / 8);
            aes.IV = derive.GetBytes(aes.BlockSize / 8);
            
            return aes;
        }
    }
}
