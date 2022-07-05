using System.Text.Json.Serialization;

namespace OktaOAuthAPI.Models
{
    public record UserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("locale")]
        public string Locale { get; set; } = string.Empty;
        [JsonPropertyName("preferred_username")]
        public string PreferredUsername { get; set; } = string.Empty;
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; } = string.Empty;
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; } = string.Empty;
        [JsonPropertyName("zoneinfo")]
        public string Zoneinfo { get; set; } = string.Empty;
    }
}