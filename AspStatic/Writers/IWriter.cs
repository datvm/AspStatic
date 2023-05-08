using System.Diagnostics.CodeAnalysis;

namespace AspStatic.Writers;

public interface IWriter
{

    Task InitializeAsync(HttpContext context);
    Task<Stream> GetOutputStreamAsync(string path);

    [DoesNotReturn]
    public static InvalidOperationException ThrowWhenNotInitialized()
    {
        throw new InvalidOperationException($"{nameof(InitializeAsync)} is not called.");
    }

}

