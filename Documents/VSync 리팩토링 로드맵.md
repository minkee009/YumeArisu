# VSync 리팩토링 로드맵

> 버전 : 0.1  
> 베이스 : .NET 9.0  
> 마지막 업데이트 : 2026-09-10  
> 작성자 : minkee009

## DXGI를 통한 프립핑 모델 차용 계획

### 1. 결정 사항
- Desktop Application -> Universal / DXGI Application으로 이름을 변경하고 그에 따른 구현을 만든다.
- DXGI는 OperatingSystem 런타임 분기에서 Windows인 경우 처리한다.
- ShaderBackend를 없애고 RenderBackend를 추가한 뒤 각각 알맞는 구현을 만든다.


## DX11.1 + OpenGL 4.1 + OpenGLES 3.0 렌더 백엔드 세분화 계획

### 1. 결정 사항
- Desktop Application -> OpenGL / D3D11 Application으로 이름을 변경하고 그에 따른 구현을 만든다.
- Android Application은 유지
- 단 ShaderBackend를 없애고 RenderBackend를 추가한 뒤 각각 알맞는 구현을 만든다.


## Vulkan 1.3 렌더링 전환 계획

### 1. 결정 사항

- 엔진 렌더러의 기본 API를 OpenGL 3.3 / OpenGLES 3.0에서 Vulkan 1.3으로 변경한다.
- Android 최소 지원 API는 Android 13 이상으로 상향한다.
- macOS Desktop은 별도의 창 클래스 계층을 두고, 필요한 경우 `Silk.NET.MoltenVK.Native`를 통해 Apple 플랫폼 전용 Vulkan/Metal 연동 경로를 사용한다.
- Windows/Linux Desktop은 `Silk.NET.Windowing` + Vulkan 인스턴스 생성 경로를 유지한다.

### 2. 현재 문제점

현재 코드의 핵심 의존성은 다음과 같다.

- `YumeArisu.Desktop/src/Implements/DesktopWindow.cs`
  - `GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ...)`
- `YumeArisu.Android/src/Implements/AndroidWindow.cs`
  - `GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Compatability, ...)`
- `YumeArisu.Core/src/Systems/RenderSystem.cs`
  - `view.CreateOpenGL()` 호출
  - `Silk.NET.OpenGL.GL` 사용
  - `ShaderBackend` enum이 OpenGL/OpenGLES만 표현

즉, 엔진의 실제 렌더 파이프라인이 GL 특화 코드에 묶여 있어 Vulkan으로 바꾸려면 단순한 설정 변경으로는 부족하다. 창 생성, 컨텍스트 생성, 셰이더 입력, 자원 관리, 화면 전송 흐름을 모두 재정의해야 한다.

### 3. 목표 아키텍처

#### 3.1 엔진 계층 분리

다음 계층을 명확하게 분리한다.

- `Windowing Layer`
  - 플랫폼별 창 생성
  - 이벤트 루프 및 Swapchain 공급
  - Vulkan 인스턴스 / Surface 연결
- `Graphics Context Layer`
  - `IVulkanDeviceContext` 또는 `IRenderDevice`
  - 장치 초기화, Queue, Swapchain, CommandBuffer, Fence, Semaphore 관리
- `Renderer Layer`
  - `SpriteRenderer`, `Camera`, `Mesh`, `Shader` 등 엔진 로직은 Vulkan 세부 사항을 직접 알지 않는다.
- `Asset / Material / Shader Layer`
  - SPIR-V 기반 셰이더 바이트코드 또는 HLSL/GLSL 컴파일 경로를 캡슐화한다.

#### 3.2 Window 설계

- `DesktopWindow`는 공통 윈도우 인터페이스를 유지한다.
- macOS는 `MacDesktopWindow` 또는 `MoltenVKDesktopWindow`로 분리한다.
- `Silk.NET.MoltenVK.Native`는 macOS 전용 Vulkan/MoltenVK 초기화에 사용한다.
- Android는 `AndroidWindow`가 Vulkan surface를 제공하도록 교체한다.

### 4. 플랫폼별 전환 전략

#### 4.1 Windows / Linux Desktop

- GLFW 또는 Silk.Windowing의 일반 루프 유지
- `ContextAPI.Vulkan` 기반 창 생성 또는 Vulkan 전용 surface 생성 경로 사용
- 창 종료/재생성 시 swapchain 재생성 로직 포함

#### 4.2 Android

- `minSdkVersion`/`targetSdkVersion`를 33 이상으로 올린다.
- `AndroidWindow`에서 `ContextAPI.OpenGLES` 설정을 제거하고 Vulkan 생성 경로로 전환한다.
- Android에서 Vulkan 표면은 `ANativeWindow` 또는 해당 API 경로와 연결해야 한다.
- Android 13 이상은 Vulkan 1.3의 호환화가 상대적으로 안정적이며, 최소 API 수준도 운영 정책과 맞아 떨어진다.

#### 4.3 macOS Desktop

- 기본 창 클래스는 공통 `DesktopWindow`로 남기고, 플랫폼 분기는 `MacDesktopWindow`로 별도 처리한다.
- Mac에서는 `Silk.NET.MoltenVK.Native`를 통해 MoltenVK/Vulkan 런타임을 연결한다.
- Metal과 Vulkan 사이의 최종 전환은 Apple 런타임 경로를 명확히 구분해야 한다.
- 지원 범위를 위해 macOS용 초기화 코드는 별도 파일로 분리하는 것이 좋다.

### 5. 실제 엔진 변경 우선순위

1. OpenGL 직접 호출 제거
   - `RenderSystem`의 `CreateOpenGL()` 및 `GL` 의존성 제거
   - `ShaderBackend` enum 삭제 또는 `GraphicsBackend.Vulkan` 추가

2. RenderContext 인터페이스 설계
   - `IRenderDevice.CreateSurface(...)`
   - `IRenderDevice.CreateSwapchain(...)`
   - `IRenderDevice.BeginFrame() / EndFrame()`

3. Vulkan 초기화 코드 추가
   - `VkInstance`, `VkPhysicalDevice`, `VkDevice`
   - queue families, surface format, present mode
   - swapchain recreation

4. Shader 시스템 전환
   - GLSL ES/GLSL 코드 경로를 SPIR-V 기반 경로로 분리
   - `Shader` 클래스에서 OpenGL 전용 코드 제거

5. 자원 관리 재설계
   - Vertex buffer, index buffer, texture upload, descriptor sets, pipeline state object

### 6. 실무 권장 사항

- 한 번에 전체 엔진을 Vulkan으로 바꾸지 말고, surface 생성 계층부터 교체한다.
- 현재 `RenderSystem`처럼 `GL` 인스턴스를 `private GL _gl;`로 직접 들고 있는 구조는 Vulkan 전환 시 가장 먼저 제거해야 한다.
- 각 플랫폼별 구현은 별도 클래스로 분리하고, 공통 인터페이스만 엔진이 알고 있도록 설계한다.
- 먼저 `IWindowControl`와 `IApplicationControl`의 계약을 유지한 채, 내부 구현만 바꾸는 방식이 가장 안전하다.

### 7. 구현 체크리스트

- [ ] `RenderSystem`에서 OpenGL 직접 의존 제거
- [ ] `DesktopWindow` / `AndroidWindow`의 API 설정 전환
- [ ] macOS 전용 창 클래스 추가
- [ ] MoltenVK 연동 조사 및 초기화 코드 구성
- [ ] Vulkan instance / device / swapchain 생성 코드 작성
- [ ] SPIR-V 셰이더 경로 정리
- [ ] Android manifest targetSdk/minSdk 검토
- [ ] Windows/macOS/Linux 런타임 검증

### 8. 결론

이번 결정은 단순한 API 교체가 아니라 엔진 렌더링 아키텍처를 다시 설계하는 작업이다. 가장 중요한 원칙은 "창 관리와 API 구현을 분리하고, Vulkan 전용 초기화 경로를 공통 인터페이스 아래 두는 것"이다.

이 방식으로 가면 Windows / Android / macOS 전환을 각각 독립적으로 유지하면서도, 최종 엔진 코드는 Vulkan 중심으로 정리할 수 있다.
