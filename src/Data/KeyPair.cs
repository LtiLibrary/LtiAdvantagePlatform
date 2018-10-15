using System;
using System.ComponentModel.DataAnnotations;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Data
{
    public class KeyPair
    {
        public KeyPair()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Required]
        [Display(Name = "ID")]
        public string Id { get; set; }

        [Display(Name = "Private Key")]
        public string PrivateKey { get; set; }

        [Display(Name = "Public Key")]
        public string PublicKey { get; set; }

        public JsonWebKey ToPublicJwt()
        {
            if (string.IsNullOrEmpty(PublicKey))
            {
                return null;
            }

            var rsaPublicKey = new RsaSecurityKey(RsaHelper.PublicKeyFromPemString(PublicKey));
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaPublicKey);
            return jwk;
        }
    }
}
