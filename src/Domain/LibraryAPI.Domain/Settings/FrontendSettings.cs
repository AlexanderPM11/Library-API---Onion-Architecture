namespace LibraryAPI.Domain.Settings
{
    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ConfirmEmailPath { get; set; } = string.Empty;
        public string ResetPasswordPath { get; set; } = string.Empty;

        public string GetConfirmEmailUrl() => $"{BaseUrl.TrimEnd('/')}/{ConfirmEmailPath.TrimStart('/')}";
        public string GetResetPasswordUrl() => $"{BaseUrl.TrimEnd('/')}/{ResetPasswordPath.TrimStart('/')}";
    }
}
