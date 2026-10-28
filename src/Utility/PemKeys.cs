using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Utility
{
    public static class PemKeys
    {
        /// <summary>
        /// Parses a PEM-encoded RSA public key ("BEGIN PUBLIC KEY" or "BEGIN RSA PUBLIC KEY").
        /// Returns null if the value is empty or not a valid RSA public key.
        /// </summary>
        public static RsaSecurityKey ParseRsaPublicKey(string pem)
        {
            if (string.IsNullOrWhiteSpace(pem))
            {
                return null;
            }

            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(pem);
                return new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false));
            }
            catch (Exception e) when (e is ArgumentException or CryptographicException)
            {
                return null;
            }
        }
    }
}
