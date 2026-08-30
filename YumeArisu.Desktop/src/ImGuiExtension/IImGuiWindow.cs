namespace YumeArisu.Desktop.ImGuiExtension;

internal interface IImGuiWindow
{
    string DisplayName { get; }
    bool IsOpen { get; set; }

    void Initialize(DebuggingUIRegistry registry);
    void Render();
}