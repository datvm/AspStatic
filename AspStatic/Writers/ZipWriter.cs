using System.IO.Compression;

namespace AspStatic.Writers;

public class ZipWriter : PathBasedWriter, IDisposable
{

    ZipArchive? archive;
    readonly Func<HttpContext, Stream> streamFunc;
    
    public ZipWriter(Func<HttpContext, Stream> streamFunc)
    {
        this.streamFunc = streamFunc;
    }

    public override async Task InitializeAsync(HttpContext context)
    {
        var stream = streamFunc(context);
        archive = new(stream, ZipArchiveMode.Create);

        await Task.CompletedTask;
    }

    protected override Task<Stream> GetStreamForPathAsync(string path)
    {
        if (archive is null)
        {
            throw IWriter.ThrowWhenNotInitialized();
        }

        var entry = archive.CreateEntry(path);

        return Task.FromResult(entry.Open());
    }

    public void Dispose()
    {
        archive?.Dispose();
    }

}
