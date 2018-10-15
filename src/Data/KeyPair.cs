using System;
using System.ComponentModel.DataAnnotations;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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

        public string ToPrivateJwt()
        {
            if (string.IsNullOrEmpty(PrivateKey))
            {
                return null;
            }

            var jwk = RsaHelper.PrivateJsonWebKeyFromPemString(PrivateKey);
            jwk.KeyId = $"{Id}-0";

            return JsonConvert.SerializeObject(jwk, Formatting.Indented);
        }

        public string ToPublicJwt()
        {
            if (string.IsNullOrEmpty(PublicKey))
            {
                return null;
            }

            var jwk = RsaHelper.PublicJsonWebKeyFromPemString(PublicKey);
            jwk.KeyId = $"{Id}-1";

            return JsonConvert.SerializeObject(jwk, Formatting.Indented);
        }
    }
}
