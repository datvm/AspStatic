namespace AspStatic.Grabbers;

public class CustomUrlsGrabber : BaseUrlGrabber
{
    readonly IEnumerable<string> urls;
    
    public CustomUrlsGrabber(IEnumerable<string> urls)
    {
        this.urls = urls;
    }

    protected override async IAsyncEnumerable<Uri> GetUrls(HttpContext context)
    {
        await Task.CompletedTask;
     
        foreach (var url in urls)
        {
            yield return new(url);
        }
    }

}