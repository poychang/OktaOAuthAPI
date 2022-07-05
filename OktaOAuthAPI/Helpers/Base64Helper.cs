using System.Text;

namespace OktaOAuthAPI.Helpers
{
    public static class Base64Helper
    {
        /// <summary>
        /// Base 64 轉碼
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Base 64 解碼
        /// </summary>
        /// <param name="base64Text"></param>
        /// <returns></returns>
        public static string Decode(string base64Text)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64Text);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
