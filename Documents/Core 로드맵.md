# Core 로드맵

## 모니터 개수 및 각 모니터 별 지원 해상도 Enum

```CSharp
using Silk.NET.GLFW;

unsafe
{
    var glfw = Glfw.GetApi();
    glfw.Init();

    // 모든 모니터 가져오기
    var monitors = glfw.GetMonitors(out int count);

    for (int i = 0; i < count; i++)
    {
        var monitor = monitors[i];

        // 해당 모니터가 지원하는 모든 비디오 모드(해상도) 가져오기
        var videoModes = glfw.GetVideoModes(monitor, out int modeCount);

        Console.WriteLine($"Monitor {i}:");
        for (int j = 0; j < modeCount; j++)
        {
            var mode = videoModes[j];
            Console.WriteLine(
                $"  {mode->Width}x{mode->Height} @ {mode->RefreshRate}Hz " +
                $"(R{mode->RedBits} G{mode->GreenBits} B{mode->BlueBits})");
        }

        // 현재 사용 중인 해상도만 보고 싶다면
        var current = glfw.GetVideoMode(monitor);
        Console.WriteLine($"  현재: {current->Width}x{current->Height} @ {current->RefreshRate}Hz");
    }
}
```


## 스카이 박스 렌더러

## 애니메이터