using System.Runtime.InteropServices;
using System.Text;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Packaging;

public static class Pak
{
    public const int VersionNum = 100; // 1.0.0
    public const long MaximumBytes = 536870912L; // 512mb
    public const long HeaderLength = 12; // byte
    public const long EntryLength = 24; // byte
    public const long ChecksumLength = 4;  // byte
    public const long EndMagicLength = 4;  // byte
    public const long FooterLength = 20; // byte (IndexOffset 8 + IndexEntryCount 4 + Checksum 4 + EndMagic 4)
    public static readonly byte[] MagicNum = [0x50, 0x41, 0x4B, 0x00]; // "PAK\0"
    public static readonly byte[] EndMagicNum = [0x4B, 0x41, 0x50, 0x00]; // "KAP\0"

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public byte[] Magic;
        public int Version;
        public int ChunkOrder;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public ulong PathId;
        public long Offset;
        public int CompressedLength;
        public int OriginalLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Footer
    {
        public long IndexOffset;
        public int IndexEntryCount;
        public uint Checksum;
        public byte[] EndMagic;
    }

    internal static ulong CalcXorSeed(ulong pathId) => Hash.Fnv1a64(BitConverter.GetBytes(pathId ^ VersionNum));
}