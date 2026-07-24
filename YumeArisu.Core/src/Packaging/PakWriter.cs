using System.Text;
using YumeArisu.Core.Utility;
using K4os.Compression.LZ4;

namespace YumeArisu.Core.Packaging;

public static class PakWriter
{
    /// <summary>
    /// 루트폴더의 이름으로 분할된 pak파일들을 만듭니다.
    /// </summary>
    /// <param name="rootFolderPath">분할 압축시킬 루트 폴더</param>
    /// <param name="outputFolderPath">Pak 파일들을 출력할 폴더</param>
    public static void Pack(string rootFolderPath, string outputFolderPath, string outputName = null, bool printProgress = true)
    {
        try
        {
            // 경로 정규화: 트레일링 슬래시 통일
            rootFolderPath = rootFolderPath.TrimEnd('/', '\\');
            outputFolderPath = outputFolderPath.TrimEnd('/', '\\');

            // output 경로가 없는 경우 생성
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            // 루트 이름 등록
            var rootName = Path.GetFileName(rootFolderPath);

            // 루트 폴더 경로 주입
            var fileEnumerable = Directory
                .EnumerateFiles(rootFolderPath, "*.*", SearchOption.AllDirectories)
                .OrderBy(x => x, StringComparer.Ordinal);
                
            Queue<string> sortedSmallFiles = new();
            Queue<string> sortedBigFiles = new();

            // 폴더 내 전체 끝 노드(파일) 조회 -> 이름 + 확장자
            // 전체 권한을 가진 파일만 있는 것으로 전제 -> 리소스 폴더 내부에는 접근권한이 자유로운 파일들만 있어야 함.
            foreach (var filePath in fileEnumerable)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);

                    if (fileInfo.Length > int.MaxValue)
                        throw new Exception("파일 너무 큼");

                    else if (fileInfo.Length > Pak.MaximumBytes - Pak.HeaderLength - Pak.FooterLength)
                        sortedBigFiles.Enqueue(filePath);

                    else
                        sortedSmallFiles.Enqueue(filePath);
                    
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("조회 에러!",ex);
                }
            }

            int totalCount = sortedSmallFiles.Count + sortedBigFiles.Count;
            int processedCount = 0;
            int currentPakNum = 0;
            string pakName = outputName ?? rootName;

            // 큐 순회로 Pak 내부 데이터 채우고 테이블 만들기
            while (sortedSmallFiles.Count > 0)
            {
                var pakPath = Path.Combine(outputFolderPath, $"{pakName}.pak{currentPakNum:D2}");
                
                WriteSmallPakFile(rootFolderPath, pakPath, ref sortedSmallFiles, 
                    (fileName) => 
                    {   
                        processedCount++;
                        if(printProgress)
                            ConsoleExtensions.PrintProgress(processedCount, totalCount, fileName);
                    }
                );

                currentPakNum++;
            }

            while (sortedBigFiles.Count > 0)
            {
                var pakPath = Path.Combine(outputFolderPath, $"{pakName}.pak{currentPakNum:D2}");

                WriteBigPakFile(rootFolderPath, pakPath, ref sortedBigFiles,
                    (fileName) => 
                    {   
                        processedCount++;
                        if(printProgress)
                            ConsoleExtensions.PrintProgress(processedCount, totalCount, fileName);
                    }
                );

                currentPakNum++;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("패키징 에러!", ex);
        }   
    }

    private static void WriteSmallPakFile(
        string rootFolderPath, 
        string outputFilePath, 
        ref Queue<string> entryFilePaths,
        Action<string> afterWriteEntry)
    {
        long currentWriteLength = 0;
        using(var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        using(var bw = new BinaryWriter(fs))
        {
            // Header
            WriteHeader(bw, new Pak.Header
            {
                Magic = [0x50, 0x41, 0x4B, 0x00], // "PAK\0"
                Version = 100
            });

            var indexEntries = new Queue<Pak.Entry>(); 
            bool overWrite = false;
            while (!overWrite && entryFilePaths.Count > 0)
            {
                var currentFile = entryFilePaths.Peek();

                // 파일경로로 원본 가져오기
                byte[] source;
                using(var i_fs = new FileStream(currentFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    source = new byte[(int)i_fs.Length]; 
                    i_fs.ReadExactly(source);
                }

                // 원본 압축
                byte[] target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
                int encodedLength = LZ4Codec.Encode(
                    source, 0, source.Length,
                    target, 0, target.Length,
                    LZ4Level.L09_HC);
                long estimatedIndexSize = (indexEntries.Count + 1) * Pak.EntryLength;
                if (currentWriteLength + encodedLength + estimatedIndexSize > Pak.MaximumBytes - Pak.HeaderLength - Pak.FooterLength)
                {
                    if (currentWriteLength == 0) // 빈 pak인데도 이 파일 하나가 안 들어감 → 정상 처리 불가능한 상황
                        throw new InvalidDataException($"파일이 너무 커서 단독으로도 pak 용량을 초과함: {currentFile}");

                    overWrite = true;
                    continue;
                }

                currentWriteLength += encodedLength;

                var relativePath = Path.GetRelativePath(rootFolderPath, currentFile)
                    .Replace('\\', '/');

                var seed = Pak.MakeEntrySeed(relativePath); 
                Crypt.Xor(target, 0, encodedLength, seed);

                indexEntries.Enqueue(
                    new Pak.Entry
                    {
                        ResourceID = Hash.Fnv1a64(Encoding.UTF8.GetBytes(relativePath)),
                        Offset = fs.Position,
                        CompressedLength = encodedLength,
                        OriginalLength = source.Length
                    }
                );

                // 바이너리 파일 만들기
                // 원본 크기 저장 (항상 리틀 엔디안으로 고정)
                bw.Write(target, 0, encodedLength);

                entryFilePaths.Dequeue();
                afterWriteEntry.Invoke(currentFile);
            }

            // Index 테이블
            int indexEntryCount = indexEntries.Count;
            long indexOffset = fs.Position;
            while (indexEntries.Count > 0)
            {
                var entry = indexEntries.Dequeue();
                WriteEntry(bw, entry);
            }

            // Footer
            WriteFooter(bw, new Pak.Footer
            {
                IndexOffset = indexOffset,
                IndexEntryCount = indexEntryCount,
                EndMagic = [0x4B, 0x41, 0x50, 0x00] // "KAP\0"
            });
        }
    }

    private static void WriteBigPakFile(
        string rootFolderPath, 
        string outputFilePath, 
        ref Queue<string> entryFilePaths,
        Action<string> afterWriteEntry)
    {
        long currentWriteLength = 0;
        using(var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        using(var bw = new BinaryWriter(fs))
        {
            // Header
            WriteHeader(bw, new Pak.Header
            {
                Magic = [0x50, 0x41, 0x4B, 0x00], // "PAK\0"
                Version = 100
            });

            var currentFile = entryFilePaths.Peek();

            // 파일경로로 원본 가져오기
            byte[] source;
            using(var i_fs = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
            {
                source = new byte[(int)i_fs.Length]; 
                i_fs.ReadExactly(source);
            }

            // 원본 압축
            byte[] target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
            int encodedLength = LZ4Codec.Encode(
                source, 0, source.Length,
                target, 0, target.Length,
                LZ4Level.L09_HC);

            currentWriteLength += encodedLength;

            var relativePath = Path.GetRelativePath(rootFolderPath, currentFile)
                .Replace('\\', '/');

            var seed = Pak.MakeEntrySeed(relativePath); 
            Crypt.Xor(target, 0, encodedLength, seed);

            var indexEntry = new Pak.Entry
            {
                ResourceID = Hash.Fnv1a64(Encoding.UTF8.GetBytes(relativePath)),
                Offset = fs.Position,
                CompressedLength = encodedLength,
                OriginalLength = source.Length
            };

            // 바이너리 파일 만들기
            // 원본 크기 저장 (항상 리틀 엔디안으로 고정)
            bw.Write(target, 0, encodedLength);

            entryFilePaths.Dequeue();
            afterWriteEntry.Invoke(currentFile);

            // Index 테이블
            int indexEntryCount = 1;
            long indexOffset = fs.Position;
            WriteEntry(bw, indexEntry);

            // Footer
            WriteFooter(bw, new Pak.Footer
            {
                IndexOffset = indexOffset,
                IndexEntryCount = indexEntryCount,
                EndMagic = [0x4B, 0x41, 0x50, 0x00] // "KAP\0"
            });
        }
    }

    private static void WriteHeader(BinaryWriter bw, Pak.Header header)
    {
        bw.Write(header.Magic);
        bw.Write(header.Version);
    }

    private static void WriteEntry(BinaryWriter bw, Pak.Entry entry)
    {
        bw.Write(entry.ResourceID);
        bw.Write(entry.Offset);
        bw.Write(entry.CompressedLength);
        bw.Write(entry.OriginalLength);
    }

    private static void WriteFooter(BinaryWriter bw, Pak.Footer footer)
    {
        bw.Write(footer.IndexOffset);
        bw.Write(footer.IndexEntryCount);
        bw.Write(footer.EndMagic);
    }
}