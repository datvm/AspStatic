using Microsoft.Extensions.Hosting;

namespace AspStatic.Grabbers;

public class WwwRootGrabber : BaseUrlGrabber
{
    protected override async IAsyncEnumerable<Uri> GetUrls(HttpContext context)
    {
        await Task.CompletedTask;
        
        var env = context.RequestServices
            .GetRequiredService<IHostEnvironment>();

        var root = env.ContentRootPath;
        if (root is null) { yield break; }

        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var path = file[(root.Length + 1)..];
            yield return new(path);
        }
    }
}