using Microsoft.AspNetCore.Http.Extensions;

namespace AspStatic.Grabbers;

sealed class UrlGrabberItem : IGrabberItem, IAsyncDisposable, IDisposable
{

    public string Path => url.LocalPath;
    public bool RequireOk { get; }

    readonly Uri url;
    HttpResponseMessage? res;
    Stream? stream;
    public UrlGrabberItem(Uri url, bool requireOk)
    {
        this.url = url;
        RequireOk = requireOk;
    }

    public void Dispose()
    {
        stream?.Dispose();
        res?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (stream is not null)
        {
            await stream.DisposeAsync();
        }

        res?.Dispose();
    }

    public async Task<Stream?> GetStreamAsync(HttpContext context)
    {
        var http = context.RequestServices
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient();

        var res = await http.GetAsync(url);
        if (RequireOk && !res.IsSuccessStatusCode)
        {
            using (res)
            {
                return null;
            }
        }

        this.res = res;
        stream = res.Content.ReadAsStream();

        return stream;
    }
}

public abstract class BaseUrlGrabber : IGrabber
{

    public bool RequireOk { get; protected set; } = true;

    public virtual async IAsyncEnumerable<IGrabberItem> GrabAsync(HttpContext context)
    {
        var urls = GetUrls(context);
        await foreach (var url in urls)
        {
            yield return new UrlGrabberItem(
                url.IsAbsoluteUri ?
                    url :
                    new(new Uri(context.Request.GetEncodedUrl()), url),
                RequireOk);
        }
    }

    protected abstract IAsyncEnumerable<Uri> GetUrls(HttpContext context);
}