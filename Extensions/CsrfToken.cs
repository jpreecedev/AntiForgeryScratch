using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AntiForgeryScratch.Extensions
{
    public static class CsrfToken
    {
        public static IApplicationBuilder UseXsrf(this IApplicationBuilder app, IAntiforgery antiforgery)
        {
            return app.Use((context, next) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions {HttpOnly = false});
                return next.Invoke();
            });
        }
    }
}