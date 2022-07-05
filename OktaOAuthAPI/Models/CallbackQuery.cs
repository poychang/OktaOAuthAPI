using System.ComponentModel;
using System.Text.Json.Serialization;

namespace OktaOAuthAPI.Models
{
    public class CallbackQuery
    {
        [JsonPropertyName("code")]
        [DefaultValue("")]
        public string Code { get; set; } = string.Empty;
        [JsonPropertyName("state")]
        [DefaultValue("")]
        public string State { get; set; } = string.Empty;
        [JsonPropertyName("error")]
        [DefaultValue("")]
        public string Error { get; set; } = string.Empty;
        [JsonPropertyName("error_description")]
        [DefaultValue("")]
        public string ErrorDescription { get; set; } = string.Empty;
    }
}
