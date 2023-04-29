using System.Net;
using System.Text;

namespace AspStatic.Middlewares;

class AspStaticBuildMiddleware
{
    static readonly SemaphoreSlim semaphore = new(1, 1);

    bool hasBuilt = false;
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
                await next(context);
                return;
            }
            hasBuilt = true;
        }
        finally
        {
            semaphore.Release();
        }

        // Build it
        string report;
        try
        {
            report = await BuildAspStaticAsync(services);
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
        await writer.WriteAsync(report);
    }

    static async Task<string> BuildAspStaticAsync(IServiceProvider services)
    {
        var report = new StringBuilder();
        var options = services.GetRequiredService<IOptions<AspStaticOptions>>().Value;

        var aspStatic = services.GetRequiredService<IAspStaticService>();
        var resources = await aspStatic.GetScanRequestAsync(options);
        await aspStatic.BuildStaticAssets(resources, options, report);

        return report.ToString();
    }

}
