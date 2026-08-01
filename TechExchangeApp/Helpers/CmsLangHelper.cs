using Microsoft.AspNetCore.Http;

namespace TechExchangeApp.Helpers
{
    /// <summary>
    /// Ngôn ngữ đang BIÊN TẬP trong CMS (chọn ở dropdown góc phải), lưu trong Session.
    /// 1 = tiếng Việt, 2 = tiếng Anh. Dùng chung cho mọi màn hình quản lý.
    /// </summary>
    public static class CmsLangHelper
    {
        public const string SessionKey = "CmsEditLang";
        public const int Vi = 1;
        public const int En = 2;

        public static int Current(HttpContext ctx)
        {
            var v = ctx.Session.GetInt32(SessionKey) ?? Vi;
            return v == En ? En : Vi;
        }

        public static void Set(HttpContext ctx, int lang)
            => ctx.Session.SetInt32(SessionKey, lang == En ? En : Vi);
    }
}
