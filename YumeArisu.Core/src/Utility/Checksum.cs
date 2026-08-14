using System.Buffers.Binary;
using System.IO.Hashing;

namespace YumeArisu.Core.Utility;

internal static class Checksum
{
    /// <summary>스트림의 0번 위치부터 length까지 CRC32를 계산합니다. (스트림 포지션 이동됨)</summary>
    public static uint ComputeCrc32(Stream stream, long length, int bufferSize = 81920)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var crc = new Crc32();
        var buffer = new byte[bufferSize];
        long remaining = length;

        while (remaining > 0)
        {
            int toRead = (int)Math.Min(bufferSize, remaining);
            int read = stream.Read(buffer, 0, toRead);
            if (read <= 0)
                throw new EndOfStreamException("체크섬 계산 중 스트림이 예상보다 짧습니다.");

            crc.Append(buffer.AsSpan(0, read));
            remaining -= read;
        }

        Span<byte> dest = stackalloc byte[4];
        crc.GetCurrentHash(dest);
        return BinaryPrimitives.ReadUInt32BigEndian(dest);
    }
}