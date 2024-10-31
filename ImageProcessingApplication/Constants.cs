using Microsoft.Owin;


namespace ImageProcessingApplication.Code
{
    public static class Constants
    {
        public static class AppSettingKeys
        {
            public const string Site = "site";
            public const string JwtSecret = "jwtsecret";
        }
        public static class ClientTypes
        {
            public const string Any = "any-client";
            public const string Telegram = "telegram";
        }

        public static class ClaimTypes
        {
            public const string IpAddress = "Tracked_ip";
            public const string Anonymous = "anonymous";
        }

        public const string AnonymousUserConstant = "anonymous";
        public const string AnonymousUserEmailConstant = AnonymousUserConstant + "@" + AnonymousUserConstant;
        public const string AnonymousUserConstantPassword = "Anonymous_1";
        public const string DeletedUserConstant = "deleted";
    }
}