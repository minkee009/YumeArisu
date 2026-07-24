using System.Text;
using YumeArisu.Core.Utility;
using K4os.Compression.LZ4;

namespace YumeArisu.Core.Packaging;

public static class PakReader
{
    private static Pak.Header ReadHeader(BinaryReader br)
    {
        return new Pak.Header
        {
            Magic = br.ReadBytes(4),   // "PAK\0" = 4바이트 고정
            Version = br.ReadInt32()
        };
    }

    private static Pak.Entry ReadEntry(BinaryReader br)
    {
        return new Pak.Entry
        {
            ResourceID = br.ReadUInt64(),
            Offset = br.ReadInt64(),
            CompressedLength = br.ReadInt32(),
            OriginalLength = br.ReadInt32()
        };
    }

    private static Pak.Footer ReadFooter(BinaryReader br)
    {
        const int footerSize = 8 + 4 + 4; // long + int + byte[4]

        br.BaseStream.Seek(-footerSize, SeekOrigin.End); // 파일 맨 끝에서 16바이트 앞으로

        return new Pak.Footer
        {
            IndexOffset = br.ReadInt64(),
            IndexEntryCount = br.ReadInt32(),
            EndMagic = br.ReadBytes(4)
        };
    }
}