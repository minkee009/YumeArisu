# Core 로드맵

> 버전 : 0.2  
> 베이스 : .NET 9.0+ 
> 마지막 업데이트 : 2026-09-10  
> 작성자 : minkee009

## 모니터 개수 및 각 모니터 별 지원 해상도 Enum
- `WindowControl`에서 현재 사용가능한 모니터들과 그 모니터들의 해상도 세트를 알려주는 값을 제공
- `Application` 초기화 단계에서 `IWindowControl` 인터페이스의 함수로 로딩


## 스카이 박스 렌더러
- 3D 씬 대비용 렌더러
- `Depth/Frame Buffer Clear` 직후 `RenderSystem`에 등록된 스카이 박스 중 `Order`가 가장 높은 것만 하나 픽업해서 처리
- 차후 부드러운 전환 기능이 추가될 경우 아래와 같은 패스로 처리될 예정
    - `ComplexSkyboxRenderer` 컴포넌트 신설
    - 마찬가지로 `Order`에 영향을 받기 때문에 해당하는 값은 컨텐츠 프로그래머가 직접 본인이 원하는 값으로 설정해야 함
    - `ComplexSkyboxRenderer` 내부에 설정된 `A`, `B` 스카이 박스로 `Lerp` 처리
    - `BRDF`, `Blinn-Phong` 등의 전역 광원 셰이더에는 3가지 옵션중 하나를 전달함.
        - `Biased` : lerp 파라미터가 0.5 이상인경우 A를 미만인 경우 B를 전달.
        - `Baked` : 렌더러가 초기화 될 때 한번 만들어낸 선형 보간 텍스쳐 5장을 통해 가장 적절한 하나의 텍스쳐를 전달함
        - `Realtime` (default) : 추가 렌더패스를 통해 A, B 안에서 보간 처리된 텍스쳐를 전달함

## 애니메이터
- 3D / 2D 복합 용 컴포넌트
- `Transform`과 같은 `GameObject`의 컴포넌트의 프로퍼티를 직접 조작할 수 있는 키프레임을 제공