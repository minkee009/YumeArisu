using System.Buffers.Binary;
using System.IO.Hashing;

namespace YumeArisu.Core.Utility;

public sealed class Crc32Stream : Stream
{
    private readonly Stream _inner;
    private readonly Crc32 _crc = new();

    public Crc32Stream(Stream inner) => _inner = inner;

    /// <summary>지금까지 이 스트림에 write된 바이트들의 누적 CRC32 값</summary>
    public uint Checksum
    {
        get
        {
            Span<byte> dest = stackalloc byte[4];
            _crc.GetCurrentHash(dest);
            return BinaryPrimitives.ReadUInt32BigEndian(dest);
        }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _crc.Append(buffer.AsSpan(offset, count));
        _inner.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _crc.Append(buffer);
        _inner.Write(buffer);
    }

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
}