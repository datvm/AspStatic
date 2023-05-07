using AspStatic.Grabbers;

namespace AspStatic.Writers;

public interface IWriter
{

    Task<Stream> GetOutputStreamAsync(IGrabberItem item);

}