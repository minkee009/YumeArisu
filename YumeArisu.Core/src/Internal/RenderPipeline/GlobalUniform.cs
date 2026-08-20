namespace YumeArisu.Core.Internal.RenderPipeline;

internal static class GlobalUniform
{
    internal const string Model = "u_Model";
    internal const string View = "u_View";
    internal const string Projection = "u_Projection";
    internal const string CameraPosition = "u_CameraPosition";

    internal const string VertexSource = $"""
        uniform mat4 {Model};
        uniform mat4 {View};
        uniform mat4 {Projection};
        """;

    internal const string FragmentSource = $"""
        uniform vec3 {CameraPosition};
        """;
}