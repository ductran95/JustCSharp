using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Http.Features;

namespace JustCSharp.AspNetCore.OpenTelemetry.Buffering;

public class ResponseBufferingStream : Stream, IHttpResponseBodyFeature
{
    private static readonly StreamPipeWriterOptions _pipeWriterOptions = new StreamPipeWriterOptions(leaveOpen: true);

    private readonly MemoryStream _memoryStream;
    private readonly Stream _innerStream;
    private readonly IHttpResponseBodyFeature _innerBodyFeature;
    private readonly Encoding _encoding;

    private PipeWriter? _pipeAdapter;

    public Stream Stream => this;

    public PipeWriter Writer => _pipeAdapter ??= PipeWriter.Create(Stream, _pipeWriterOptions);
    
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

    public ResponseBufferingStream(IHttpResponseBodyFeature innerBodyFeature, Encoding encoding)
    {
        _innerBodyFeature = innerBodyFeature;
        _innerStream = _innerBodyFeature.Stream;
        _encoding = encoding;
        _memoryStream = new MemoryStream();
    }

    public ResponseBufferingStream(IHttpResponseBodyFeature innerBodyFeature) : this(innerBodyFeature, Encoding.UTF8)
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

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _innerStream.Write(buffer);
        _memoryStream.Write(buffer);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        await _memoryStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        await _innerStream.WriteAsync(buffer, cancellationToken);
        await _memoryStream.WriteAsync(buffer, cancellationToken);
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

    public void DisableBuffering()
    {
        _innerBodyFeature.DisableBuffering();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _innerBodyFeature.StartAsync(cancellationToken);
    }

    public Task SendFileAsync(string path, long offset, long? count,
        CancellationToken cancellationToken = default)
    {
        return _innerBodyFeature.SendFileAsync(path, offset, count, cancellationToken);
    }

    public Task CompleteAsync()
    {
        return _innerBodyFeature.CompleteAsync();
    }
    
    public string ReadText()
    {
        return _encoding.GetString(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length);
    }
}