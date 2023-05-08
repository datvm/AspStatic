namespace AspStatic.Writers;

public abstract class PathBasedWriter : IWriter
{

    public virtual Task InitializeAsync(HttpContext context)
    {
        return Task.CompletedTask;
    }

    public virtual async Task<Stream> GetOutputStreamAsync(string path)
    {
        while (path[0] == '/')
        {
            path = path[1..];
        }

        var fileName = Path.GetFileName(path);
        if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            path += ".html";
        }
        else if (!fileName.Contains('.'))
        {
            path = Path.Combine(path, "index.html");
        }

        path = path.Replace('\\', '/');

        return await GetStreamForPathAsync(path);
    }

    protected abstract Task<Stream> GetStreamForPathAsync(string path);

}
