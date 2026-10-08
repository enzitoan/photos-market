using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PhotosMarket.API.Filters;

/// <summary>
/// Agrega Cache-Control y ETag a las respuestas GET exitosas y responde 304
/// cuando el cliente ya tiene la misma versión. La caché es "private" porque
/// algunas respuestas dependen del código de acceso del álbum.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ClientCacheAttribute : Attribute, IAsyncResultFilter
{
    private readonly int _seconds;

    public ClientCacheAttribute(int seconds)
    {
        _seconds = seconds;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (HttpMethods.IsGet(request.Method) &&
            context.Result is ObjectResult { Value: not null, StatusCode: null or 200 } result)
        {
            var response = context.HttpContext.Response;
            var etag = ComputeETag(result.Value);

            response.Headers.CacheControl = $"private, max-age={_seconds}";
            response.Headers.ETag = etag;

            if (MatchesIfNoneMatch(request.Headers.IfNoneMatch.ToString(), etag))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
            }
        }

        await next();
    }

    private static string ComputeETag(object value)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType());
        var hash = SHA256.HashData(json);
        return $"W/\"{Convert.ToHexString(hash, 0, 16)}\"";
    }

    private static bool MatchesIfNoneMatch(string ifNoneMatch, string etag)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatch))
            return false;

        if (ifNoneMatch.Trim() == "*")
            return true;

        return ifNoneMatch
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(candidate => string.Equals(StripWeakPrefix(candidate), StripWeakPrefix(etag), StringComparison.Ordinal));
    }

    private static string StripWeakPrefix(string tag) =>
        tag.StartsWith("W/", StringComparison.Ordinal) ? tag[2..] : tag;
}
