using System.Text;
using Android.Content;
using Android.Content.Res;
using YumeArisu.Core.Abstractions;
using YumeArisu.Core.Packaging;
using YumeArisu.Core.Utility;

namespace YumeArisu.Android.Implements;

/// <summary>
/// Helper to assist with file loading.
/// </summary>
public sealed class AndroidFileIO : IFileIO
{
    private AssetManager _assetManager;
    private Context _context;

    public bool IsOpened { get; private set; } // _disposed 역할 겸비 
    private List<Stream> _pakChunkStreams;
    private Dictionary<ulong, PakReader.MetaData> _metaDataTable;

    public AndroidFileIO(AssetManager assets, Context context)
    {
        _assetManager = assets;
        _context = context;
    }

    internal void Open(string pakRootFolder, string pakName)
    {
        if (IsOpened)
            return;

        _pakChunkStreams = new();

        long versionCodeRaw;
        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            versionCodeRaw = _context.PackageManager
                .GetPackageInfo(_context.PackageName, 0)
                .LongVersionCode;
        }
        else
        {
        #pragma warning disable CS0618
            versionCodeRaw = _context.PackageManager
                .GetPackageInfo(_context.PackageName, 0)
                .VersionCode;
        #pragma warning restore CS0618
        }

        string appVersion = versionCodeRaw.ToString();

        string targetDir = Path.Combine(_context.FilesDir.AbsolutePath, pakRootFolder);
        string versionMarkerPath = Path.Combine(targetDir, ".version");

        // 저장된 버전과 현재 앱 버전이 다르면(또는 최초 실행이면) 통째로 밀고 새로 복사
        bool needsCopy = !File.Exists(versionMarkerPath) 
            || File.ReadAllText(versionMarkerPath) != appVersion;

        if (needsCopy)
        {
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, recursive: true);

            Directory.CreateDirectory(targetDir);
        }

        try
        {
            int chunkOrder = 0;

            while (true)
            {
                string fileName = $"{pakName}.pak{chunkOrder:D2}";
                string assetPath = $"{pakRootFolder}/{fileName}";
                string localPath = Path.Combine(targetDir, fileName);

                if (needsCopy)
                {
                    try
                    {
                        using var assetStream = _assetManager.Open(assetPath, Access.Streaming);
                        using var fileStream = File.Create(localPath);
                        assetStream.CopyTo(fileStream);
                    }
                    catch
                    {
                        break; // 더 이상 청크 없음
                    }
                }
                else if (!File.Exists(localPath))
                {
                    break; // 캐시는 있는데 이 청크만 없음 -> 여기서 종료
                }

                _pakChunkStreams.Add(File.OpenRead(localPath));
                chunkOrder++;
            }

            if (needsCopy)
                File.WriteAllText(versionMarkerPath, appVersion);

            if (_pakChunkStreams.Count == 0)
                throw new FileNotFoundException($"'{pakName}'에 해당하는 PAK 청크 파일을 찾을 수 없습니다.");

            var streamView = new ReadOnlyListView<Stream>(_pakChunkStreams);
            PakReader.ExtractMetaDataTable(streamView, out _metaDataTable);

            IsOpened = true;
        }
        catch
        {
            if (_pakChunkStreams != null)
            {
                foreach (var stream in _pakChunkStreams)
                    stream.Dispose();

                _pakChunkStreams.Clear();
                _pakChunkStreams = null;
            }

            _metaDataTable?.Clear();
            _metaDataTable = null;

            throw;
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
        _pakChunkStreams = null;
        _metaDataTable?.Clear();
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
