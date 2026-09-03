using YumeArisu.Core.Internal.ResourceHandling;

namespace YumeArisu.Core.Common;

public class RawText : Resource
{
    public string Text { get; private set; }

    protected override bool OnLoad(byte[] bytes)
    {
        Text = System.Text.Encoding.UTF8.GetString(bytes);
        return true;
    }

    protected override void OnUnload()
    {
        Text = null;
    }
}