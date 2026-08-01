using System.Text;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Utility;
using YumeArisu.Core.Packaging;

namespace YumeArisu.Desktop.Implements;

public sealed class DesktopFileIO : IFileIO
{
    public bool IsOpened { get; private set; } // _disposed 역할 겸비 
    
    private List<Stream> _pakChunkStreams;
    private Dictionary<ulong, PakReader.MetaData> _metaDataTable;

    internal void Open(string pakRootFolder, string pakName)
    {
        if (IsOpened)
        {
            ConsoleExtensions.WriteLineColored($"FileIO가 이미 열려있습니다.", ConsoleColor.Yellow);
            return;
        }

        if (!Directory.Exists(pakRootFolder))
            throw new DirectoryNotFoundException($"PAK 루트 폴더를 찾을 수 없습니다: {pakRootFolder}");

        _pakChunkStreams = new();

        try
        {
            int chunkOrder = 0;

            while (true)
            {
                string chunkFileName = $"{pakName}.pak{chunkOrder:D2}";
                string chunkFilePath = Path.Combine(pakRootFolder, chunkFileName);

                if (!File.Exists(chunkFilePath))
                    break;

                var stream = new FileStream(chunkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _pakChunkStreams.Add(stream);

                chunkOrder++;
            }

            if (_pakChunkStreams.Count == 0)
                throw new FileNotFoundException($"'{pakName}'에 해당하는 PAK 청크 파일을 찾을 수 없습니다.");

            var streamView = new ReadOnlyListView<Stream>(_pakChunkStreams);
            PakReader.ExtractMetaDataTable(streamView, out _metaDataTable);

            IsOpened = true;
        }
        catch
        {
            // 오픈 과정 실패 시 열어둔 스트림 정리
            if (_pakChunkStreams != null)
            {
                foreach (var stream in _pakChunkStreams)
                    stream.Dispose();

                _pakChunkStreams.Clear();
                _pakChunkStreams = null;
            }

            _metaDataTable?.Clear();
            _metaDataTable = null;

            throw; // 예외 다시 던지기
        }
    }

    internal void Close()
    {
        if (!IsOpened)
        {
            ConsoleExtensions.WriteLineColored($"FileIO가 열리지 않은 상태에서 닫기 요청이 호출되었습니다.", ConsoleColor.Yellow);
            return;
        }

        // 스트림 닫기 + 멤버 필드 비우기
        foreach (var stream in _pakChunkStreams)
            stream.Close();

        _pakChunkStreams?.Clear();
        _metaDataTable?.Clear();
        _pakChunkStreams = null;
        _metaDataTable = null;

        IsOpened = false;
    }

    public bool Exists(string path)
    {
        if (!IsOpened || _metaDataTable == null)
            return false;

        var relativePath = path.Replace('\\', '/');
        var pathId = Hash.Fnv1a64(Encoding.UTF8.GetBytes(relativePath));

        return _metaDataTable.ContainsKey(pathId);
    }

    public byte[] ReadAllBytes(string path)
    {
        if (!IsOpened)
            throw new Exception("아직 FileIO가 열리지 않았습니다!");

        return PakReader.Unpack(path, _pakChunkStreams, _metaDataTable);
    }

    public string ReadAllString(string path)
    {
        if (!IsOpened)
            throw new Exception("아직 FileIO가 열리지 않았습니다!");

        byte[] bytes = ReadAllBytes(path);
        return Encoding.UTF8.GetString(bytes);
    }

    public void Dispose()
    {
        if (IsOpened)
        {
            Close();
        }
    }
}