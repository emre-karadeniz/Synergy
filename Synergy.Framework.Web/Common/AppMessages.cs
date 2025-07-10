namespace Synergy.Framework.Web.Common;

public static class AppMessages
{
    public static class Common
    {
        public const string Add = "The registration process was successful.";
        public const string Delete = "The deletion was successful.";
        public const string Update = "The update was successful.";
        public const string Error = "An error occurred during the process!";
        public const string Success = "The operation was completed successfully.";
        public const string NotFound = "No such record found.";
        public const string RecordUsed = "This record is in use. It cannot be deleted.";
        public const string RateLimiting = "You have sent too many requests. Please wait a moment.";
    }

    public static class Login
    {
        public const string LoginSucceed = "Login successful.";
        public const string LoginFailed = "Username or password is incorrect.";
        public const string AccessDenied = "Access denied: You do not have the required permissions to perform this action.";
        public const string ValidToken = "A valid token was not provided. Please log in.";
        public const string RegisterFailed = "This username is in use.";
    }

    public static class Validation
    {
        public static string Required(string propertyName) => $"{propertyName} is required.";
        public static string NotFound(string propertyName) => $"{propertyName} was not found.";
    }
}
