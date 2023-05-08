namespace AspStatic.Services;

public interface IAspStaticService
{
    Task BuildAsync();
}

class AspStaticService : IAspStaticService
{

    readonly IOptions<AspStaticOptions> options;
    readonly HttpContext context;
    public AspStaticService(IOptions<AspStaticOptions> options, IHttpContextAccessor context)
    {
        this.options = options;
        this.context = context.HttpContext ??
            throw new NullReferenceException();
    }

    public async Task BuildAsync()
    {
        var o = options.Value;
        if (o.Grabbers.Count == 0)
        {
            throw new InvalidOperationException($"There is no {nameof(o.Grabbers)}");
        }

        if (o.Writers.Count == 0)
        {
            throw new InvalidOperationException($"There is no {nameof(o.Writers)}");
        }

        foreach (var writer in o.Writers)
        {
            await writer.InitializeAsync(context);
        }

        foreach (var grabber in o.Grabbers)
        {
            await foreach (var item in grabber.GrabAsync(context))
            {
                await using var htmlStream = await item.GetStreamAsync(context);
                if (htmlStream is null) { continue; }

                var outputStreams = await Task.WhenAll(
                    o.Writers
                        .Select(q => q.GetOutputStreamAsync(item.Path)));
                await using var writer = new MultiStreamWriter(outputStreams);

                await htmlStream.CopyToAsync(writer);
            }

            await DisposeIfAvailableAsync(grabber);
        }

        foreach (var writer in o.Writers)
        {
            await DisposeIfAvailableAsync(writer);
        }
    }

    static async Task DisposeIfAvailableAsync<T>(T obj)
    {
        switch (obj)
        {
            case IAsyncDisposable ad:
                await ad.DisposeAsync();
                break;
            case IDisposable d:
                d.Dispose();
                break;
        }
    }

}
