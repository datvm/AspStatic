namespace AspStatic.Writers;

internal class MultiStreamWriter : Stream
{
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotImplementedException();
    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    readonly IEnumerable<Stream> streams;
    public MultiStreamWriter(IEnumerable<Stream> streams)
    {
        this.streams = streams;
    }

    public override void Flush()
    {
        foreach (var stream in streams)
        {
            stream.Flush();
        }
    }

    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        foreach (var stream in streams)
        {
            await stream.FlushAsync();
        }
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotImplementedException();

    public override void SetLength(long value) =>
        throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
        foreach (var s in streams)
        {
            s.Write(buffer, offset, count);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            foreach (var s in streams)
            {
                s.Dispose();
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        foreach (var s in streams)
        {
            await s.DisposeAsync();
        }
    }

}
