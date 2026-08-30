namespace YumeArisu.Desktop.ImGuiExtension;

internal interface IImGuiWindow
{
    internal string DisplayName { get; }
    internal bool IsOpen { get; set; }

    internal void Initialize(DebuggingUIRegistry registry);
    internal void Render();
}