namespace AspStatic.Middlewares;

class AspStaticUrlGathererMiddleware
{

    readonly RequestDelegate next;
    public AspStaticUrlGathererMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider services)
    {
        var service = services.GetRequiredService<IUrlGathererService>();

        string url = context.Request.GetDisplayUrl();
        await next.Invoke(context);

        int statusCode = 0;
        try
        {
            statusCode = context.Response.StatusCode;
        }
        catch { }

        service.Add(new(url, (HttpStatusCode)statusCode));
    }

}
