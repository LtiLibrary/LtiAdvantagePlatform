using Microsoft.AspNetCore.Http;

namespace AdvantagePlatform.Utility
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// The issuer of this platform, exactly as OpenIddict writes it into the
        /// "iss" claim of id_tokens and into /.well-known/openid-configuration
        /// (scheme, host, path base, and a trailing slash). Tools compare "iss"
        /// strings exactly, so use this value everywhere the issuer is shown or sent.
        /// </summary>
        public static string GetIssuer(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}{request.PathBase}".EnsureTrailingSlash();
        }
    }
}
