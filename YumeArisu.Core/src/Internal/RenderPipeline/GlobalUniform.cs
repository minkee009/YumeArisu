namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class GlobalUniform
{
    internal const string Model = "uModel";
    internal const string View = "uView";
    internal const string Projection = "uProjection";
    internal const string CameraPosition = "uCameraPosition";

    internal const string VertexSource = $"""
        uniform mat4 {Model};
        uniform mat4 {View};
        uniform mat4 {Projection};
        """;

    internal const string FragmentSource = $"""
        uniform vec3 {CameraPosition};
        """;
}