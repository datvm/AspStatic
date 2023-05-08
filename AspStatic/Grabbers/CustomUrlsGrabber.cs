namespace AspStatic.Grabbers;

public class CustomUrlsGrabber : BaseUrlGrabber
{
    readonly Func<HttpContext, Task<IEnumerable<string>>> urls;

    public CustomUrlsGrabber(Func<HttpContext, Task<IEnumerable<string>>> urls)
    {
        this.urls = urls;
    }

    protected override async IAsyncEnumerable<Uri> GetUrls(HttpContext context)
    {
        var urls = await this.urls(context);

        foreach (var url in urls)
        {
            yield return new(url, UriKind.RelativeOrAbsolute);
        }
    }

}