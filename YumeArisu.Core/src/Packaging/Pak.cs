using System.Runtime.InteropServices;
using System.Text;
using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Packaging;

public static class Pak
{
    public const int VersionNum = 100; // 1.0.0
    public const long MaximumBytes = 536870912L; // 512mb
    public const long HeaderLength = 8; // byte
    public const long EntryLength = 24; // byte
    public const long FooterLength = 16; // byte
    public static readonly byte[] MagicNum = [0x50, 0x41, 0x4B, 0x00]; // "PAK\0"
    public static readonly byte[] EndMagicNum = [0x4B, 0x41, 0x50, 0x00]; // "KAP\0"

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public byte[] Magic;
        public int Version;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public ulong ResourceID;
        public long Offset;
        public int CompressedLength;
        public int OriginalLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Footer
    {
        public long IndexOffset;
        public int IndexEntryCount;
        public byte[] EndMagic;
    }

    internal static ulong CalcXorSeed(string relativePath)
    {
        // Key + 파일 상대경로를 합쳐서 파일별로 유일한 seed 생성
        byte[] keyBytes = BitConverter.GetBytes(VersionNum);
        byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);

        byte[] combined = new byte[keyBytes.Length + pathBytes.Length];
        Buffer.BlockCopy(keyBytes, 0, combined, 0, keyBytes.Length);
        Buffer.BlockCopy(pathBytes, 0, combined, keyBytes.Length, pathBytes.Length);

        return Hash.Fnv1a64(combined);
    }
}