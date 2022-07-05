namespace OktaOAuthAPI.Models
{
    public record AppSettings
    {
        public Okta Okta { get; set; } = default!;
    }

    public record Okta
    {
        public string Url { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
}
