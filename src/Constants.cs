namespace AdvantagePlatform
{
    public static class Constants
    {
        public static class EndpointAuthenticationMethods
        {
            public const string SignedJwt = "signed_jwt";
        }

        public static class ParsedSecretTypes
        {
            public const string SignedJwt = "SignedJwt";
        }

        public static class ProtocolRoutePaths
        {
            public const string Token = "connect/token";
        }

        public static class SecretTypes
        {
            public const string PrivateKey = "PrivateKey";
            public const string PublicKey = "PublicKey";
        }
    }
}
