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

        var wwwRoot = Path.Combine(root, "wwwroot");

        var files = Directory.EnumerateFiles(wwwRoot, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var path = "/" + file[(wwwRoot.Length + 1)..].Replace('\\', '/');
            yield return new(path, UriKind.RelativeOrAbsolute);
        }
    }
}