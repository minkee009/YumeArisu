using ImGuiNET;
using System.Numerics;
using YumeArisu.Core.Hierarchy;

namespace YumeArisu.Desktop.ImGuiExtension;

internal class InspectorWindow : IImGuiWindow
{
    private DebuggingUIRegistry _registry;

    public void Initialize(DebuggingUIRegistry registry)
    {
        _registry = registry;
    }

    public void Render()
    {
        ImGui.Begin("Inspector###InspectorWindow");

        var selected = _registry.GetData<GameObject>(DebuggingUIKeys.SelectedGameObject);

        if (selected is null)
        {
            ImGui.TextDisabled("선택된 오브젝트가 없습니다.");
            ImGui.End();
            return;
        }

        if (selected.IsDestroyed)
        {
            ImGui.TextDisabled("선택된 오브젝트가 파괴되었습니다.");
            _registry.ClearData(DebuggingUIKeys.SelectedGameObject);
            ImGui.End();
            return;
        }

        DrawGeneralInfo(selected);
        ImGui.Separator();
        DrawTransform(selected.Transform);
        ImGui.Separator();
        DrawComponents(selected);

        ImGui.End();
    }

    private void DrawGeneralInfo(GameObject go)
    {
        ImGui.Text($"이름 : {go.Name}");
        ImGui.Text($"ID : {go.ID}");

        string tag = go.Tag ?? "";
        if (ImGui.InputText("Tag", ref tag, 64))
            go.Tag = tag;

        int layer = (int)go.Layer;
        if (ImGui.InputInt("Layer", ref layer))
            go.Layer = (uint)Math.Max(0, layer);

        bool activeSelf = go.ActiveSelf;
        if (ImGui.Checkbox("ActiveSelf", ref activeSelf))
            go.SetActive(activeSelf);

        ImGui.SameLine();
        ImGui.TextDisabled($"(ActiveInHierarchy : {go.ActiveInHierarchy})");
    }

    private void DrawTransform(Transform tr)
    {
        ImGui.Text("Transform (Local)");

        var pos = tr.LocalPosition;
        if (ImGui.DragFloat3("Position", ref pos, 0.1f))
            tr.LocalPosition = pos;

        var euler = QuaternionToEuler(tr.LocalRotation);
        if (ImGui.DragFloat3("Rotation", ref euler, 1.0f))
            tr.LocalRotation = EulerToQuaternion(euler);

        var scale = tr.LocalScale;
        if (ImGui.DragFloat3("Scale", ref scale, 0.01f))
            tr.LocalScale = scale;

        if (tr.Parent is not null)
        {
            ImGui.TextDisabled($"World Position : {tr.WorldPosition}");
        }
    }

    private void DrawComponents(GameObject go)
    {
        ImGui.Text($"Components ({go.Components.Count})");

        foreach (var comp in go.Components)
            ImGui.BulletText(comp.GetType().Name);
    }

    // ImGui가 쿼터니언을 직접 지원 안 하므로 표시/편집용 오일러 변환 (단순 근사, 짐벌락 케어 안 함)
    private static Vector3 QuaternionToEuler(Quaternion q)
    {
        Vector3 angles;

        float sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        float cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = MathF.Atan2(sinrCosp, cosrCosp);

        float sinp = 2 * (q.W * q.Y - q.Z * q.X);
        angles.Y = MathF.Abs(sinp) >= 1
            ? MathF.CopySign(MathF.PI / 2, sinp)
            : MathF.Asin(sinp);

        float sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = MathF.Atan2(sinyCosp, cosyCosp);

        return angles * (180f / MathF.PI);
    }

    private static Quaternion EulerToQuaternion(Vector3 eulerDegrees)
    {
        var rad = eulerDegrees * (MathF.PI / 180f);
        return Quaternion.CreateFromYawPitchRoll(rad.Y, rad.X, rad.Z);
    }
}