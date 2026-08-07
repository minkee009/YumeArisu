using YumeArisu.Core.Utility;

namespace YumeArisu.Core.Systems;

public readonly struct NoConfig { }

/// <summary>
/// Core에서 공통으로 사용하는 전역 시스템 베이스입니다.
/// CRTP(Curiously Recurring Template Pattern)를 사용하여 싱글톤 패턴을 구현합니다.
/// 
/// SystemBase에서 사용되는 주된 스텝용 함수의 이름은 다음과 같이 작성해야 합니다.
/// 
///  - BeginFrame() : 프레임 시작 부분에 호출해야 함
///  - EndFrame() : 프레임 끝 부분에 호출해야 함
/// 
/// </summary>
/// <typeparam name="T">CRTP를 위한 본인의 타입입니다.</typeparam>
/// <typeparam name="TConfig">초기화 함수 StartUp에 전달할 인자의 형식입니다.</typeparam>
public abstract class SystemBase<T, TConfig> where T : SystemBase<T, TConfig>, new()
{
    public static T Instance => _instance ??= new T();

    public bool IsStarted { get; private set; }
    
    private static T _instance;

    internal abstract void OnStartUp(TConfig config); 
    internal abstract void OnShutDown(); 

    public void StartUp(TConfig config)
    {
        if (IsStarted)
        {
            ConsoleExtensions.WriteLineColored(
                $"{typeof(T).Name}는 이미 StartUp 되었습니다. 중복 호출을 확인하세요.",
                ConsoleColor.Yellow);
            return;
        }

        OnStartUp(config);
        IsStarted = true;
    }

    public void ShutDown()
    {
        if (!IsStarted)
        {
            ConsoleExtensions.WriteLineColored(
                $"{typeof(T).Name}는 StartUp되지 않은 상태에서 ShutDown이 호출되었습니다.",
                ConsoleColor.Yellow);
            return;
        }

        OnShutDown();
        IsStarted = false;

        if (_instance == this)
            _instance = null;
    }
}