using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OktaOAuthAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OktaOAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthorizeController : ControllerBase
    {
        private readonly ILogger<AuthorizeController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly AppSettings _config;
        private readonly HttpClient _httpClient;

        /// <summary>建構式</summary>
        public AuthorizeController(ILogger<AuthorizeController> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IOptions<AppSettings> config)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _config = config.Value;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_config.Okta.Url);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // GET: api/Authorize
        /// <summary>轉向到 Okta 做身分驗證</summary>
        /// <param name="callback">呼叫此身分驗證 API 的來源網址，完成身分驗證之後再轉向回此 URL</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAuthorize([FromQuery(Name = "callback")] string callback)
        {
            // INFO: https://developer.okta.com/docs/reference/api/oidc/#authorize
            var ticket = GenerateCallbackUrlTicket(callback);

            if (string.IsNullOrEmpty(ticket)) return BadRequest("Failed to generate callback ticket.");

            var uri = $"{_config.Okta.Url}/oauth2/v1/authorize" +
                $"?response_type=code" +
                $"&client_id={_config.Okta.ClientId}" +
                $"&redirect_uri={_config.Okta.RedirectUri}" +
                $"&scope={_config.Okta.Scope}" +
                $"&state={ticket}";

            return Redirect(uri);

            // 將 guid 和 callback URL 寫入存放區，完成身分驗證之後轉向回此 URL
            string GenerateCallbackUrlTicket(string callbackUrl)
            {
                var key = Guid.NewGuid().ToString();
                var expirationTime = TimeSpan.FromSeconds(100);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(expirationTime);

                if (_memoryCache.TryGetValue(key, out string _)) return "NO_STATE";
                _memoryCache.Set(key, callbackUrl, cacheEntryOptions);

                return key.ToString();
            }
        }

        // GET: api/Authorize/Callback
        /// <summary>Okta 驗證完後轉向到此位置，取得使用者身分驗證的 code</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <param name="state">驗證用，可用於跨網站保護，避免 CSRF 攻擊</param>
        /// <param name="error">錯誤訊息</param>
        /// <param name="errorDescription">錯誤描述</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Callback")]
        public async Task<IActionResult> GetCallback([FromQuery] CallbackQuery model)
        {
            // 透過 state 取得對應的 callbackUrl
            if (!_memoryCache.TryGetValue(model.State, out string callbackUrl))
                return new JsonResult(new { Error = "Can not find callback url.", model.State, ErrorDescription = "The state ticket is expired." });
            // 檢查 Okta OAuth 是否有發生錯誤
            if (!string.IsNullOrEmpty(model.Error))
                return new JsonResult(new { model.Error, model.State, model.ErrorDescription });

            var accessToken = await FetchAccessToken(model.Code);
            var userInfo = await FetchUserInfo(accessToken);
            
            await RevokeAccessToken(accessToken);

            return Redirect(callbackUrl + "?userinfo=" + JsonSerializer.Serialize(userInfo));
        }
    }
}