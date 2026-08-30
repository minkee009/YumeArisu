namespace YumeArisu.Desktop.ImGuiExtension;

internal sealed class DebuggingUIRegistry
{
    private sealed class RegistryData
    {
        internal Type Type { get; }
        internal object Data { get; }

        internal RegistryData(Type type, object data)
        {
            Type = type;
            Data = data;
        }
    }

    private readonly Dictionary<string, RegistryData> _dataMap = new();

    internal void SetData(string name, object data)
    {
        _dataMap[name] = new(data.GetType(), data);
    }

    internal void ClearData(string name)
    {
        _dataMap.Remove(name);
    }

    internal T GetData<T>(string name) where T : class
    {
        if (_dataMap.TryGetValue(name, out var entry) && entry.Data is T match)
            return match;

        return null;
    }

    internal void Clear()
    {
        _dataMap.Clear();
    }
}