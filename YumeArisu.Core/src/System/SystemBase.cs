using System;
using YumeAris.Core.Utility;

namespace YumeAris.Core.System;

public readonly struct NoConfig { }
/// <summary>
/// OS와 면밀한 연관이 있는 전역 객체를 위한 베이스입니다.
/// 
/// SystemBase에서 사용되는 주된 스텝용 함수의 이름은 다음과 같이 작성해야 합니다.
/// 
///  - Update() : 호출 순서와 상관 없음
///  - BeginFrame() : 프레임 시작 부분에 호출해야 함
///  - EndFrame() : 프레임 끝 부분에 호출해야 함
/// 
/// </summary>
/// <typeparam name="T">CRTP를 위한 본인의 타입입니다.</typeparam>
/// <typeparam name="TConfig">초기화 함수 StartUp에 전달할 인자의 형식입니다.</typeparam>
public abstract class SystemBase<T, TConfig> where T : SystemBase<T, TConfig>, new()
{
    private static T _instance;

    public static T Instance => _instance ??= new T();

    public bool IsStarted { get; private set; }

    public abstract void StartUpInternal(TConfig config); 
    public abstract void ShutDownInternal(); 

    public void StartUp(TConfig config)
    {
        if (IsStarted)
        {
            ConsoleColorExtensions.WriteLineColored(
                $"{typeof(T).Name}는 이미 StartUp 되었습니다. 중복 호출을 확인하세요.",
                ConsoleColor.Red);
            return;
        }

        StartUpInternal(config);
        IsStarted = true;
    }

    public void ShutDown()
    {
        if (!IsStarted)
        {
            ConsoleColorExtensions.WriteLineColored(
                $"{typeof(T).Name}는 StartUp되지 않은 상태에서 ShutDown이 호출되었습니다.",
                ConsoleColor.Red);
            return;
        }

        ShutDownInternal();
        IsStarted = false;

        if (_instance == this)
            _instance = null;
    }
}