using ImGuiNET;
using YumeArisu.Core.Systems;

namespace YumeArisu.Desktop.ImGuiExtension;

internal class TimeWindow : IImGuiWindow
{
    public string DisplayName => "Time";
    public bool IsOpen { get; set; } = false;

    public void Initialize(DebuggingUIRegistry registry) { }

    public void Render()
    {
        if (!IsOpen)
            return;

        bool isOpen = IsOpen;
        ImGui.Begin("Time###TimeWindow", ref isOpen);
        IsOpen = isOpen;

        float timeScale = Time.TimeScale;
        if (ImGui.SliderFloat("Time Scale", ref timeScale, 0f, 3f))
            TimeSystem.Instance.TimeScale = timeScale;

        ImGui.SameLine();
        if (ImGui.Button(timeScale <= 0f ? "Resume" : "Pause"))
            TimeSystem.Instance.TimeScale = timeScale <= 0f ? 1f : 0f;

        ImGui.Separator();

        float fixedDeltaTime = Time.FixedDeltaTime;
        if (ImGui.DragFloat("Fixed Delta Time", ref fixedDeltaTime, 0.001f, 0.0001f, 1f))
            TimeSystem.Instance.FixedDeltaTime = fixedDeltaTime;

        ImGui.Separator();

        ImGui.Text($"FPS : {ImGui.GetIO().Framerate:F0}");
        ImGui.Text($"Frame Time : {ImGui.GetIO().DeltaTime * 1000f:F2} ms");
        ImGui.Text($"Delta Time : {Time.DeltaTime:F4}s");
        ImGui.Text($"Unscaled Delta Time : {Time.UnscaledDeltaTime:F4}s");
        ImGui.Text($"Total Time : {Time.TotalTime:F1}s");
        ImGui.Text($"Fixed Time : {Time.FixedTime:F1}s");
        ImGui.Text($"Frame Count : {Time.FrameCount}");

        ImGui.End();
    }
}