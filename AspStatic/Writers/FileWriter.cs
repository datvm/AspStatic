using AspStatic.Grabbers;

namespace AspStatic.Writers;

public class FileWriter : IWriter
{
    readonly string outFolder;
    public FileWriter(string outFolder)
    {
        this.outFolder = outFolder;
        Directory.CreateDirectory(outFolder);
    }

    public async Task<Stream> GetOutputStreamAsync(IGrabberItem item)
    {
        await Task.CompletedTask;
        var path = Path.Combine(outFolder, item.Path);
        
        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        var stream = File.Create(path);
        return stream;
    }

}
