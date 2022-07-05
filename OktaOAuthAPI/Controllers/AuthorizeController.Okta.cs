using Microsoft.AspNetCore.Mvc;
using OktaOAuthAPI.Helpers;
using OktaOAuthAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OktaOAuthAPI.Controllers
{
    public partial class AuthorizeController : ControllerBase
    {
        /// <summary>取得 Okta Access Token</summary>
        /// <param name="code">用來取得 Access Token 的 Authorize Code</param>
        /// <returns></returns>
        private async Task<string> FetchAccessToken(string code)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Helper.Encode($"{_config.Okta.ClientId}:{_config.Okta.ClientSecret}"));
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _config.Okta.RedirectUri),
            });
            var response = await _httpClient.PostAsync("oauth2/v1/token", content);
            var data = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(data).RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        }

        /// <summary>
        /// 審查 Okta Access Token 狀態
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private async Task<string> IntrospectAccessToken(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Helper.Encode($"{_config.Okta.ClientId}:{_config.Okta.ClientSecret}"));
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", accessToken),
                new KeyValuePair<string, string>("token_type_hint", "access_token"),
            });
            var response = await _httpClient.PostAsync("oauth2/v1/introspect", content);
            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) return data;
            else
            {
                // TODO: handle error
                return data;
            }
        }

        /// <summary>
        /// 註銷 Okta Access Token
        /// </summary>
        /// <param name="accessToken">Okta Access Token</param>
        /// <returns></returns>
        private async Task<string> RevokeAccessToken(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Base64Helper.Encode($"{_config.Okta.ClientId}:{_config.Okta.ClientSecret}"));
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", accessToken),
                new KeyValuePair<string, string>("token_type_hint", "access_token"),
            });
            var response = await _httpClient.PostAsync("oauth2/v1/revoke", content);
            var data = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) return data;
            else
            {
                // TODO: handle error
                return data;
            }
        }

        /// <summary>
        /// 取得 Okta 中使用者的基本資訊
        /// </summary>
        /// <param name="accessToken">適用於指定使用者的 Access Token</param>
        /// <returns></returns>
        private async Task<UserInfo> FetchUserInfo(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("oauth2/v1/userinfo");
            var data = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<UserInfo>(data) ?? new UserInfo();
        }
    }
}