using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using YumeArisu.Core.Abstractions;

namespace YumeArisu.Desktop.Implements;

public sealed class DesktopFileIO : IFileIO
{
    public FileIOFeatures Capabilities => throw new NotImplementedException();

    internal DesktopFileIO()
    {
        
    }

    internal void Open()
    {
        
    }

    internal void Close()
    {
        
    }

    public byte[] ReadAllBytes(string path)
    {
        throw new NotImplementedException();
    }

    public string ReadAllString(string path)
    {
        throw new NotImplementedException();
    }
}