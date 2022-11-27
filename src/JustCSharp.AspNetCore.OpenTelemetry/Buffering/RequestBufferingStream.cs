using System.Text;

namespace JustCSharp.AspNetCore.OpenTelemetry.Buffering;

public class RequestBufferingStream : Stream
{
    private readonly Stream _innerStream;
    private readonly MemoryStream _memoryStream;
    private readonly Encoding _encoding;

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set
        {
            _innerStream.Position = value;
            _memoryStream.Position = value;
        }
    }
    
    public RequestBufferingStream(Stream innerStream, Encoding encoding)
    {
        _innerStream = innerStream;
        _encoding = encoding;
        _memoryStream = new MemoryStream();
    }
    
    public RequestBufferingStream(Stream innerStream): this(innerStream, Encoding.UTF8)
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var value = _innerStream.Seek(offset, origin);
        _memoryStream.Seek(offset, origin);
        return value;
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
        _memoryStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
        _memoryStream.Write(buffer, offset, count);
    }

    public override void Flush()
    {
        _innerStream.Flush();
        _memoryStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var res = _innerStream.Read(buffer, offset, count);
        _memoryStream.Write(buffer.AsSpan(offset, res));
        return res;
    }
    
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var res = await _innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        _memoryStream.Write(buffer.AsSpan(offset, res));
        return res;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        var res = await _innerStream.ReadAsync(destination, cancellationToken);
        _memoryStream.Write(destination.Slice(0, res).Span);
        return res;
    }

    protected override void Dispose(bool disposing)
    {
        _memoryStream.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _memoryStream.DisposeAsync();
        await base.DisposeAsync();
    }

    public string ReadText()
    {
        return _encoding.GetString(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length);
    }
}