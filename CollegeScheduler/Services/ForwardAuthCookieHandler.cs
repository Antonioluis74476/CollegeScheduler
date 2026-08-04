namespace CollegeScheduler.Services;

public sealed class ForwardAuthCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;

    public ForwardAuthCookieHandler(IHttpContextAccessor http)
        => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = _http.HttpContext;

        // Forward browser cookies (Identity auth cookie) to the API call
        if (ctx is not null && ctx.Request.Headers.TryGetValue("Cookie", out var cookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", cookie.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}