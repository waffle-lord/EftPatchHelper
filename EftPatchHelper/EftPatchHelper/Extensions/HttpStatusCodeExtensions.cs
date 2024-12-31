using System.Net;

namespace EftPatchHelper.Extensions;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccessStatus(this HttpStatusCode statusCode) => statusCode is >= HttpStatusCode.OK and <= HttpStatusCode.Ambiguous;
}