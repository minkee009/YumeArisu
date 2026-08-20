using System.Text;
using Silk.NET.OpenGL;
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

    internal bool ImmediatLoadFromReference(VertexLayout layout, float[] vertices, uint[] indices)
    {
        if (IsLoaded)
            return false;

        IsLoaded = InitializeMesh(layout, vertices, indices);
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

        return InitializeMesh(meta.Layout, meta.Vertices, meta.Indices);
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
    }

    private bool InitializeMesh(VertexLayout layout, float[] vertices, uint[] indices)
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

        return true;
    }
}