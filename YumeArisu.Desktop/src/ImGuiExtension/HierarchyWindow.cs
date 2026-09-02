using ImGuiNET;
using YumeArisu.Core.Hierarchy;
using YumeArisu.Core.Systems;

namespace YumeArisu.Desktop.ImGuiExtension;

internal class HierarchyWindow : IImGuiWindow
{
    public string DisplayName => "Hierarchy";
    public bool IsOpen { get; set; } = true;

    private DebuggingUIRegistry _registry;

    public void Initialize(DebuggingUIRegistry registry)
    {
        _registry = registry;
    }

    public void Render()
    {
        if (!IsOpen)
            return;

        bool isOpen = IsOpen;
        ImGui.Begin("Hierarchy###SceneInfoWindow", ref isOpen);
        IsOpen = isOpen;

        var currentScene = SceneControl.CurrentScene;
        if (currentScene != null && ImGui.CollapsingHeader(currentScene.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
        {
            DrawSceneGameObjects(currentScene);
        }

        var staticScene = SceneSystem.Instance.StaticScene;
        if (staticScene != null && ImGui.CollapsingHeader(staticScene.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
        {
            DrawSceneGameObjects(staticScene);
        }

        ImGui.End();
    }

    private void DrawSceneGameObjects(Scene scene)
    {
        foreach (var go in scene.GameObjects)
        {
            if (go.Transform.Parent is null)
                DrawGameObjectNode(go);
        }
    }

    private void DrawGameObjectNode(GameObject go)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        if (go.Transform.ChildCount == 0)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        bool isSelected = _registry.GetData<GameObject>(DebuggingUIKeys.SelectedGameObject) == go;
        if (isSelected)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (!go.ActiveInHierarchy)
            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1f));

        bool opened = ImGui.TreeNodeEx($"{go.Name}###GO_{go.ID}", flags);

        if (!go.ActiveInHierarchy)
            ImGui.PopStyleColor();

        // 화살표가 아니라 라벨 자체를 클릭했을 때 선택 처리
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && !ImGui.IsItemToggledOpen())
            _registry.SetData(DebuggingUIKeys.SelectedGameObject, go);

        if (go.Transform.ChildCount > 0 && opened)
        {
            foreach (var child in go.Transform.Children)
                DrawGameObjectNode(child.GameObject);

            ImGui.TreePop();
        }
    }
}