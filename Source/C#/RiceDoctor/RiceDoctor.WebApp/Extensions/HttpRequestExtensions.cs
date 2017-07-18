using Microsoft.AspNetCore.Http;

namespace RiceDoctor.WebApp
{
    public static class HttpRequestExtensions
    {
        public static string GetBaseUrl(this HttpRequest request)
        {
            return $@"{request.Scheme}://{request.Host}/Class?className=";
        }
    }
}