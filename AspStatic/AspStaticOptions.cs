using Microsoft.Extensions.Hosting;

namespace AspStatic;

public class AspStaticOptions
{

    public List<IGrabber> Grabbers { get; } = new();
    public List<IWriter> Writers { get; } = new();

    public AspStaticOptions()
    {
        GrabWwwRootFiles();
        GrabRazorPages();
    }

    public AspStaticOptions GrabWwwRootFiles(bool requireSuccessfulResponse = true)
    {
        Grabbers.Add(new WwwRootGrabber()
        {
            RequireOk = requireSuccessfulResponse,
        });
        return this;
    }

    public AspStaticOptions GrabRazorPages(bool requireSuccessfulResponse = true)
    {
        Grabbers.Add(new RazorPagesGrabber()
        {
            RequireOk = requireSuccessfulResponse,
        });
        return this;
    }

    public AspStaticOptions GrabCustomUrls(IEnumerable<string> uris, bool requireSuccessfulResponse = true) =>
        GrabCustomUrls(_ => Task.FromResult(uris), requireSuccessfulResponse);

    public AspStaticOptions GrabCustomUrls(Func<HttpContext, Task<IEnumerable<string>>> uris, bool requireSuccessfulResponse = true)
    {
        Grabbers.Add(new CustomUrlsGrabber(uris)
        {
            RequireOk = requireSuccessfulResponse,
        });
        return this;
    }

    public AspStaticOptions WriteToFolder(string relativePathToContentRootPath, bool clearBeforeBuild)
    {
        Writers.Add(new FsWriter((ctx) =>
        {
            var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
            return Path.Combine(env.ContentRootPath, relativePathToContentRootPath);
        }, clearBeforeBuild));

        return this;
    }

    public AspStaticOptions WriteToFolder(Func<HttpContext, string> folderFunc, bool clearBeforeBuild)
    {
        Writers.Add(new FsWriter(folderFunc, clearBeforeBuild));

        return this;
    }

    public AspStaticOptions WriteToAbsoluteFolder(string absoluteFolderPath, bool clearBeforeBuild) =>
        WriteToFolder(_ => absoluteFolderPath, clearBeforeBuild);

    public AspStaticOptions WriteToZip(string outputZipPath, bool relativeToContentRootPath = true) =>
        WriteToZip(ctx =>
        {
            var path = outputZipPath;

            if (relativeToContentRootPath)
            {
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
                path = Path.Combine(env.ContentRootPath, outputZipPath);
            }

            return File.Create(path);
        });

    public AspStaticOptions WriteToZip(Stream outputStream) =>
        WriteToZip(_ => outputStream);

    public AspStaticOptions WriteToZip(Func<HttpContext, Stream> outputStreamFunc)
    {
        Writers.Add(new ZipWriter(outputStreamFunc));
        return this;
    }

}
