namespace AspStatic.Writers;

public class FsWriter : PathBasedWriter
{
    readonly Func<HttpContext, string> folderFunc;
    readonly bool clearBeforeBuild;

    string? absoluteFolder;
    public FsWriter(Func<HttpContext, string> folderFunc, bool clearBeforeBuild)
    {
        this.folderFunc = folderFunc;
        this.clearBeforeBuild = clearBeforeBuild;
    }

    protected override async Task<Stream> GetStreamForPathAsync(string path)
    {
        if (absoluteFolder is null)
        {
            await Task.CompletedTask;
            throw IWriter.ThrowWhenNotInitialized();
        }

        var filePath = Path.Combine(absoluteFolder, path);
        var dir = Path.GetDirectoryName(filePath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        var stream = File.Create(filePath);
        return stream;
    }

    public override async Task InitializeAsync(HttpContext context)
    {
        absoluteFolder = folderFunc(context);

        if (clearBeforeBuild && Directory.Exists(absoluteFolder))
        {
            Directory.Delete(absoluteFolder, true);
        }
        Directory.CreateDirectory(absoluteFolder);

        await Task.CompletedTask;
    }
}
