using YumeArisu.Core.Utility;
using K4os.Compression.LZ4;
using System.Buffers;
using System.Text;

namespace YumeArisu.Core.Packaging;

public static class PakReader
{
    public struct MetaData
    {
        public int ChunkOrder;
        public long Offset;
        public int CompressedLength;
        public int OriginalLength;
    }

    /// <summary>
    /// 팩 청크스트림을 모두 읽고 경로해쉬로 정리된 메타데이터 테이블을 추출합니다.
    /// </summary>
    /// <param name="pakChunkStreams">팩 청크 스트림 리스트</param>
    /// <param name="metaDataTable">메타데이터 테이블</param>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="Exception"></exception>
    public static void ExtractMetaDataTable(ReadOnlyListView<Stream> pakChunkStreams, out Dictionary<ulong, MetaData> metaDataTable)
    {
        metaDataTable = new();

        for (int i = 0; i < pakChunkStreams.Count; i++)
        {
            var ps = pakChunkStreams[i];
            using (var br = new BinaryReader(ps, Encoding.UTF8, true))
            {
                // Header 읽기
                ps.Seek(0, SeekOrigin.Begin);
                var header = ReadHeader(br);

                // i가 chunkOrder과 일치하는 지 체크
                if (i != header.ChunkOrder)
                    throw new InvalidDataException("PAK 청크 오더 불일치");

                // Magic 검증
                if (!header.Magic.SequenceEqual(Pak.MagicNum))
                    throw new InvalidDataException("PAK 매직 넘버 불일치");

                if (header.Version != Pak.VersionNum)
                    throw new InvalidDataException($"지원하지 않는 버전: {header.Version}");

                // Footer 읽기
                ps.Seek(-Pak.FooterLength, SeekOrigin.End); // 파일 맨 끝에서 16바이트 앞으로
                var footer = ReadFooter(br);

                // EndMagic 검증
                if (!footer.EndMagic.SequenceEqual(Pak.EndMagicNum))
                    throw new InvalidDataException("PAK 엔드 매직 넘버 불일치");

                // Checksum 검증
                long checksumRegionLength = ps.Length - Pak.EndMagicLength - Pak.ChecksumLength;
                uint computedChecksum = ChecksumUtility.ComputeCrc32(ps, checksumRegionLength);
                if (computedChecksum != footer.Checksum)
                    throw new InvalidDataException("PAK 체크섬 불일치: 파일이 손상되었을 수 있습니다");

                // Pak Index Entry 읽기
                ps.Seek(footer.IndexOffset, SeekOrigin.Begin);                
                for (int j = 0; j < footer.IndexEntryCount; j++)
                {
                    var entry = ReadEntry(br);

                    if (entry.Offset < 0 || entry.Offset >= ps.Length)
                        throw new InvalidDataException("잘못된 Offset");

                    if (entry.CompressedLength <= 0 || 
                        entry.CompressedLength > ps.Length)
                        throw new InvalidDataException("잘못된 크기");

                    var pathId = entry.PathId;
                    var metaData = new MetaData
                    {
                        ChunkOrder = i,
                        Offset = entry.Offset,
                        CompressedLength = entry.CompressedLength,
                        OriginalLength = entry.OriginalLength
                    };

                    metaDataTable.Add(pathId, metaData);
                }
            }
        }
    }

    /// <summary>
    /// 경로 문자열과 팩 청크스트림 리스트 뷰, 메타데이터 테이블로 팩 청크스트림 내부의 소스 오프셋을 찾아 사용 가능한 바이트배열 형태로 복원하여 반환합니다.
    /// </summary>
    /// <returns></returns>
    public static byte[] Unpack(string path, ReadOnlyListView<Stream> pakChunkStreams, in Dictionary<ulong, MetaData> metaDataTable)
    {
        // GC 비용 줄이기 위한 Buffer View
        byte[] compressed = null;
        byte[] original = null;

        try
        {
            var relativePath = path.Replace('\\', '/');

            var pathId = Hash.Fnv1a64(Encoding.UTF8.GetBytes(relativePath));
            var metaData = metaDataTable[pathId];
            var pakChunkStream = pakChunkStreams[metaData.ChunkOrder];

            pakChunkStream.Seek(metaData.Offset, SeekOrigin.Begin);
            compressed = ArrayPool<byte>.Shared.Rent(metaData.CompressedLength);

            pakChunkStream.ReadExactly(compressed, 0, metaData.CompressedLength);

            ulong seed = Pak.CalcXorSeed(pathId);
            Crypt.Xor(compressed, 0, metaData.CompressedLength, seed);

            original = ArrayPool<byte>.Shared.Rent(metaData.OriginalLength);

            var decodedLength = LZ4Codec.Decode(
                compressed, 0, metaData.CompressedLength,
                original, 0, metaData.OriginalLength
            );

            if (decodedLength != metaData.OriginalLength)
                throw new InvalidDataException("압축 해제 실패");
            
            // 반환용은 새 객체로 생성 (GC 할당 있음)
            byte[] result = new byte[metaData.OriginalLength];
            Buffer.BlockCopy(original, 0, result, 0, metaData.OriginalLength);

            return result;
        }
        finally
        {
            // 예외가 발생하든 정상 종료되든 빌려온 메모리는 반드시 반납
            if (compressed is not null) ArrayPool<byte>.Shared.Return(compressed);
            if (original is not null) ArrayPool<byte>.Shared.Return(original);
        }
    }

    private static Pak.Header ReadHeader(BinaryReader br)
    {
        return new Pak.Header
        {
            Magic = br.ReadBytes(4),
            Version = br.ReadInt32(),
            ChunkOrder = br.ReadInt32()
        };
    }

    private static Pak.Entry ReadEntry(BinaryReader br)
    {
        return new Pak.Entry
        {
            PathId = br.ReadUInt64(),
            Offset = br.ReadInt64(),
            CompressedLength = br.ReadInt32(),
            OriginalLength = br.ReadInt32()
        };
    }

    private static Pak.Footer ReadFooter(BinaryReader br)
    {
        return new Pak.Footer
        {
            IndexOffset = br.ReadInt64(),
            IndexEntryCount = br.ReadInt32(),
            Checksum = br.ReadUInt32(),
            EndMagic = br.ReadBytes(4)
        };
    }
}