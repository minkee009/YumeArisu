namespace YumeArisu.Desktop.ImGuiExtension;

internal interface IImGuiWindow
{
    void Initialize(DebuggingUIRegistry registry);
    void Render();
}