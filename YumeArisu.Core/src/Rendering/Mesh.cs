using System.Text;
using System.Numerics;
using Silk.NET.OpenGL;
using YumeArisu.Core.Common;
using YumeArisu.Core.Internal.RenderPipeline;
using YumeArisu.Core.Internal.ResourceHandling;
using YumeArisu.Core.Systems;
using YumeArisu.Core.Utility;

public class Mesh : Resource
{
    internal ulong VertexLayoutID { get; private set; }    
    internal uint VAOHandle { get; private set; }
    internal uint VBOHandle { get; private set; }
    internal uint EBOHandle { get; private set; }
    internal uint IndexCount { get; private set; }
    public BoundingBox Bounds { get; private set; }

    internal unsafe void DrawElements()
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.BindVertexArray(VAOHandle);
        gl.DrawElements(GLEnum.Triangles, IndexCount, DrawElementsType.UnsignedInt, null);
        gl.BindVertexArray(0);
    }

    internal bool ImmediatLoadFromReference(VertexLayout layout, float[] vertices, uint[] indices)
    {
        if (IsLoaded)
            return false;

        IsLoaded = InitializeMesh(layout, vertices, indices, CalculateBounds(layout, vertices));
        return IsLoaded;
    }

    protected override bool OnLoad(byte[] bytes)
    {
        // .mesh 파일인지 확인
        if (!PathHelper.HasExtension(Path, ".mesh"))
            return false;

        // material meta로 전환
        var json = Encoding.UTF8.GetString(bytes);
        var meta = JsonMetaParser.Parse<MeshMeta>(json);

        return InitializeMesh(meta.Layout, meta.Vertices, meta.Indices, meta.Bounds);
    }

    protected override void OnUnload()
    {
        var gl = RenderSystem.Instance.GetGL();
        gl.DeleteVertexArray(VAOHandle);
        gl.DeleteBuffer(VBOHandle);
        gl.DeleteBuffer(EBOHandle);

        VAOHandle = 0;
        VBOHandle = 0;
        EBOHandle = 0;
        VertexLayoutID = 0;
        IndexCount = 0;
        Bounds = default;
    }

    private bool InitializeMesh(VertexLayout layout, float[] vertices, uint[] indices, BoundingBox bounds)
    {
        var gl = RenderSystem.Instance.GetGL();

        VAOHandle = gl.GenVertexArray();
        VBOHandle = gl.GenBuffer();
        EBOHandle = gl.GenBuffer();

        gl.BindVertexArray(VAOHandle);
        gl.BindBuffer(GLEnum.ArrayBuffer, VBOHandle);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBOHandle);

        unsafe
        {
            fixed (void* ptr = &vertices[0])
                gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
            
            fixed (void* ptr = &indices[0])
                gl.BufferData(GLEnum.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);

            int stride = layout.GetStride();
            int offset = 0;

            foreach (var element in layout.Elements)
            {
                gl.EnableVertexAttribArray((uint)element.Location);

                gl.VertexAttribPointer(
                    (uint)element.Location,
                    element.Type.GetComponentCount(),
                    GLEnum.Float,
                    false,
                    (uint)stride,
                    (void*)offset);

                offset += element.Type.GetSize();
            }
        }
        
        gl.BindVertexArray(0);

        VertexLayoutID = layout.GetID();
        IndexCount = (uint)indices.Length;
        Bounds = bounds;

        return true;
    }

    private static BoundingBox CalculateBounds(VertexLayout layout, float[] vertices)
    {
        int stride = layout.GetStride() / sizeof(float);
        int positionOffset = 0;

        foreach (var element in layout.Elements)
        {
            if (string.Equals(element.Name, "Position", StringComparison.OrdinalIgnoreCase))
                break;

            positionOffset += element.Type.GetSize() / sizeof(float);
        }

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (int index = positionOffset; index + 2 < vertices.Length; index += stride)
        {
            var position = new Vector3(vertices[index], vertices[index + 1], vertices[index + 2]);
            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        return min == new Vector3(float.MaxValue) ? default : new BoundingBox(min, max);
    }
}