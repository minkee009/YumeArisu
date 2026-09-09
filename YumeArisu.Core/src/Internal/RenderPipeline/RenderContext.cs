using System.Numerics;
using Silk.NET.OpenGL;
using YumeArisu.Core.Rendering;
using YumeArisu.Core.Systems;

namespace YumeArisu.Core.Internal.RenderPipeline;

internal sealed class RenderContext
{
    private readonly GL _gl;

    internal RenderContext(GL gl)
    {
        _gl = gl;
    }

    internal void UpdateCameraContext(Camera camera)
    {
        GlobalShaderProperties.UpdateCamera(camera.ViewMatrix, camera.ProjectionMatrix, camera.Transform.WorldPosition);
    }

    internal void Submit(in RenderCommand command)
    {
        command.Material.ApplyRenderState();
        command.Material.Apply(command.MaterialOverride);
        foreach (var (name, value) in command.ObjectProperties.Properties)
            command.Material.Shader.SetShaderProperty(name, value);

        int unit = command.MaterialOverride.Textures.Count;
        foreach (var (name, texture) in command.ObjectProperties.Textures)
            command.Material.Shader.SetTexture(name, texture, unit++);

        Draw(command.Mesh);
    }

    internal unsafe void Draw(Mesh mesh)
    {
        _gl.BindVertexArray(mesh.VAOHandle);
        _gl.DrawElements(GLEnum.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, null);
        _gl.BindVertexArray(0);
    }
}