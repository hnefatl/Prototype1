using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;

namespace WebService
{
    public static class Crypto
    {
        public static byte[] Hash(string In)
        {
            const int SaltLength = 16;

            using (RNGCryptoServiceProvider Gen = new RNGCryptoServiceProvider())
            {
                using (SHA512 Provider = new SHA512Managed())
                {
                    System.Text.Encoding Encoder = System.Text.Encoding.UTF8;

                    byte[] Plain = new byte[SaltLength + Encoder.GetByteCount(In)];

                    byte[] Salt = new byte[SaltLength];
                    Gen.GetBytes(Salt);
                    Array.Copy(Salt, Plain, Salt.Length);

                    Encoder.GetBytes(In, 0, In.Length, Plain, SaltLength);

                    return Provider.ComputeHash(Plain);
                }
            }
        }
    }
}