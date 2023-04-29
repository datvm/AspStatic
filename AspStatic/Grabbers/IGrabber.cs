namespace AspStatic.Grabbers;

public interface IGrabber
{
    IAsyncEnumerable<IGrabberItem> GrabAsync(HttpContext context);
}

public delegate Task<Stream?> GetStreamAsync(HttpContext context);

public interface IGrabberItem
{
    string Path { get; }
    Task<Stream?> GetStreamAsync(HttpContext context);
}