# 렌더 패스 - RenderQueue 각 스테이지

> 버전 : 1.0  
> 베이스 : .NET 9.0+ 
> 마지막 업데이트 : 2026-09-10  
> 작성자 : minkee009

`RenderQueue`는 렌더링의 큰 단계를 나타내는 enum입니다. 값이 작은 단계부터 먼저 렌더링됩니다.

현재 렌더 파이프라인은 범용 메시 렌더러가 아니라 `SpriteRenderer` 중심으로 구현되어 있습니다. 따라서 현재 사용하지 않는 렌더 단계는 미리 정의하지 않고, 실제 기능이 필요해질 때 추가합니다.

## 현재 적용된 스테이지

| 이름 | 값 | 현재 용도 | 상태 |
| --- | ---: | --- | --- |
| `Geometry` | `2000` | 커스텀 셰이더 머터리얼의 불투명 또는 알파 클리핑 Sprite | 적용됨 |
| `Transparent` | `3000` | 기본 Sprite를 포함한 알파 블렌딩 Sprite | 적용됨 |
| `UI` | `4000` | UI 렌더링을 위한 예약 단계 | enum만 정의됨 |

현재 `Material.RenderQueue`의 타입은 `RenderQueue` enum입니다. 기본 Material은 `Geometry`, 기본 Sprite Material은 `Transparent`를 사용합니다. 따라서 현재 기본 Sprite 렌더 패스는 알파값 처리 가능성을 고려해 전부 `Transparent`로 처리됩니다.

`Geometry` Sprite는 기본 Sprite 셰이더를 사용하는 배치 경로가 아니라 커스텀 셰이더 머터리얼을 사용하는 경우에만 해당합니다. 이 경우 렌더 시스템은 셰이더의 알파 처리 방식을 알 수 없으므로, 불투명 처리 또는 알파 클리핑을 커스텀 셰이더가 직접 구현해야 합니다.

```text
기본 Sprite Material
→ Transparent
→ 기본 Sprite 셰이더와 SpriteBatcher 사용

커스텀 셰이더 Material
→ Material.RenderQueue를 직접 선택
→ Geometry인 경우 불투명/알파 클리핑을 셰이더가 직접 처리
→ Transparent인 경우 머터리얼의 블렌딩과 투명 정렬 정책 사용
```

```csharp
public enum RenderQueue
{
	Geometry = 2000,
	Transparent = 3000,
	UI = 4000
}
```

`SpriteRenderer`의 기본 정렬 순서는 다음과 같습니다.

```text
RenderQueue
→ RenderOrder
→ 등록 순서
```

반투명 Sprite는 위 단계에 더해 카메라 공간의 깊이를 사용해 먼 Sprite부터 가까운 Sprite 순서로 처리합니다. 불투명 Sprite는 DepthTest와 DepthMask를 사용해 깊이 버퍼가 앞뒤 관계를 결정합니다.

## 아직 적용되지 않은 확장 계획

다음 항목은 현재 코드에 구현되어 있지 않습니다.

| 이름 | 계획 값 | 계획 용도 |
| --- | ---: | --- |
| `Background` | `1000` | 배경 전용 렌더 패스 |
| `AlphaTest` | `2450` | 알파 클리핑을 별도 렌더 단계로 분리하는 경우 |
| `UI` 렌더 패스 | `4000` | 화면 좌표 기반 UI 렌더러와 오버레이 |
| 범용 투명 렌더러 | - | Sprite 이외의 투명 오브젝트 |

현재는 Geometry 커스텀 셰이더가 알파 클리핑을 직접 처리하므로 별도의 `AlphaTest` 단계가 필요하지 않습니다. 이후 알파 클리핑을 엔진 공통 기능으로 제공하거나 별도 패스로 분리할 필요가 생기면 그때 추가합니다.

확장할 때는 해당 렌더러와 렌더 패스를 실제로 구현한 뒤 enum 항목을 추가합니다. enum 값만 미리 늘려 두지 않습니다.

## 사용 규칙

`RenderQueue`는 세부 정렬값이 아니라 큰 렌더링 단계를 구분하는 값입니다. 현재는 enum이므로 다음과 같은 정수 오프셋 방식은 사용하지 않습니다.

```csharp
// 지원하지 않음
RenderQueue.Transparent + 10
```

같은 단계 안의 세부 순서가 필요하면 해당 렌더러의 정책을 사용합니다.

```text
SpriteRenderer:
RenderQueue
→ RenderOrder
→ 반투명인 경우 카메라 공간 깊이
→ 등록 순서
```

새로운 스테이지가 필요해지면 다음을 함께 수정합니다.

1. `RenderQueue` enum에 새 단계를 추가합니다.
2. 해당 렌더러 또는 렌더 패스를 구현합니다.
3. Material 메타데이터와 스키마를 갱신합니다.
4. 이 문서의 현재 적용 표를 갱신하고, 구현 전이라면 확장 계획 표에 둡니다.
