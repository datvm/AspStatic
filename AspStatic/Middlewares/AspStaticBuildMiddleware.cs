using System.Net;

namespace AspStatic.Middlewares;

class AspStaticBuildMiddleware
{
    static readonly SemaphoreSlim semaphore = new(1, 1);
    static bool hasBuilt = false;

    readonly RequestDelegate next;
    public AspStaticBuildMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider services)
    {
        // Make sure to do it only once
        try
        {
            await semaphore.WaitAsync();

            if (hasBuilt)
            {
                semaphore.Release();
                await next(context);
                return;
            }

            hasBuilt = true;
            semaphore.Release();
        }
        catch
        {
            semaphore.Release();
            throw;
        }

        // Build it
        try
        {
            var aspStaticService = services.GetRequiredService<IAspStaticService>();
            await aspStaticService.BuildAsync();
        }
        catch (Exception)
        {
            hasBuilt = false;
            throw;
        }

        // Response
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.Created;
        await using var writer = new StreamWriter(context.Response.Body);
        await writer.WriteAsync("ASP Static built.");
    }

}
