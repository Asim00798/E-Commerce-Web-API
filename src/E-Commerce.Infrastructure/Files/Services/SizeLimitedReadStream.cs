namespace E_Commerce.Infrastructure.Files.Services;

public sealed class SizeLimitedReadStream : Stream
{
    private readonly Stream _inner;
    private readonly long _maxBytes;
    private long _totalBytesRead;

    public SizeLimitedReadStream(Stream inner, long maxBytes)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _maxBytes = maxBytes;
    }

    public long TotalBytesRead => _totalBytesRead;

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => _totalBytesRead;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        if (read > 0)
            TrackBytes(read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var read = await _inner.ReadAsync(buffer, cancellationToken);
        if (read > 0)
            TrackBytes(read);
        return read;
    }

    private void TrackBytes(int count)
    {
        _totalBytesRead += count;
        if (_totalBytesRead > _maxBytes)
            throw new InvalidOperationException(
                $"File size exceeds the allowed maximum of {_maxBytes} bytes.");
    }

    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _inner.Dispose();
        base.Dispose(disposing);
    }
}