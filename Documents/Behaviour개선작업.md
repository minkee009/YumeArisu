
## 핵심 포인트

가장 큰 병목은 “이벤트가 실행될 때마다 전체 Behaviour를 다시 훑는 구조”와 “상태 확인 시마다 부모 계층을 따라가며 활성 상태를 재계산하는 구조”입니다.

## 개선 포인트

1. Awake/Start를 매 프레임 전체 스캔하는 구조
- BehaviourSystem.cs 의 `ExecuteAwake()`와 `ExecuteStart()`는 `State`와 `ActiveInHierarchy`를 확인하기 위해 매 프레임 전체 리스트를 순회합니다.
- 하지만 `Awake`와 `Start`는 보통 한 번만 실행되어야 하므로, 매 프레임 전체 탐색은 불필요한 비용입니다.
- 개선 방향:
  - “실행 대기 중인 Behaviour”를 따로 큐/리스트로 관리
  - 한 번 실행된 항목은 즉시 제거하거나 상태를 갱신

2. 활성 상태 체크가 매 프레임마다 반복적으로 일어남
- `CheckChangeState()`에서 매 프레임마다 `_behaviours` 전체를 순회하고, 각 항목마다 `IsActiveAndEnabled`를 확인합니다.
- 이 과정에서 `IsActiveAndEnabled`가 다시 `GameObject.ActiveInHierarchy`를 참조하므로, 부모 계층을 따라가며 재계산이 발생합니다.
- 개선 방향:
  - 활성 상태를 캐시하고, enable/disable 또는 부모 활성 변화 시에만 갱신
  - “상태가 바뀐 Behaviour만” 처리하는 구조로 바꾸기

3. `GameObject.ActiveInHierarchy`의 재귀적 계산 비용
- GameObject.cs 에서 `ActiveInHierarchy`는 부모 체인을 따라가며 계산합니다.
- BehaviourSystem의 여러 이벤트 함수에서 이 값을 자주 읽기 때문에, 전체적으로 매우 자주 호출되는 핫 패스가 됩니다.
- 개선 방향:
  - `ActiveInHierarchy`를 캐시
  - 부모의 활성 상태가 바뀔 때만 invalidate/refresh

4. `HashSet` 기반 활성 목록의 비용
- `_activeBehaviours`가 `HashSet<Behaviour>`라서, 상태 전이 시 `Contains/Add/Remove`가 반복됩니다.
- 이 구조는 “중복 제거”에는 좋지만, 이벤트 호출용으로는 해시 연산 오버헤드가 있고, 순회도 리스트보다 덜 cache-friendly합니다.
- 개선 방향:
  - `List<Behaviour>` 기반으로 바꾸거나
  - “활성/비활성 분리 리스트” 구조로 바꾸기

5. 이벤트 전환용 큐가 매번 가비지/오버헤드를 유발함
- `RegisterBehaviour()`와 `UnregisterBehaviour()`가 `Queue<Action>`와 람다를 사용합니다.
- `Action` 람다를 생성하고 큐에 넣는 행위는 컴포넌트 등록/해제 시점에 불필요한 할당을 만들 수 있습니다.
- 개선 방향:
  - `Action` 대신 작은 구조체/플래그 기반의 pending op 구조 사용
  - `Queue<T>` 대신 `List<T>` + 인덱스로 관리

6. OnDestroy 처리 시 리스트 제거가 반복적으로 일어남
- `ExecuteOnDestroy()`가 뒤에서 앞으로 순회하면서 `RemoveAt(i)`를 사용합니다.
- 삭제가 많아질수록 리스트 요소 이동 비용이 커져서, 최악의 경우 비용이 급격히 증가합니다.
- 개선 방향:
  - destroy 대상을 먼저 수집한 뒤 한 번에 제거
  - 삭제 후 compact를 한 번에 처리

7. Update/FixedUpdate/LateUpdate의 virtual 호출 비용
- `FixedUpdate()`, `Update()`, `LateUpdate()`는 모두 가상 메서드 호출입니다.
- Behaviour가 실제로 override하지 않는 경우에도 호출 자체는 남아 있어, 이벤트가 많은 객체에서 비용이 누적됩니다.
- 개선 방향:
  - “override 여부”를 미리 표시하는 플래그/덱스 사용
  - override된 경우에만 호출하는 방식으로 분기 최적화

## 우선순위 추천

가장 효과가 클 가능성이 높은 순서로 정리하면:

1. Awake/Start의 전체 스캔 제거
2. `ActiveInHierarchy` 캐시
3. 상태 전이용 큐/리스트 구조 단순화
4. 활성 Behaviour 집합을 `HashSet` 대신 더 가벼운 구조로 변경
5. Destroy 처리 방식 개선

원하시면 다음 단계로, 이 분석을 바탕으로 “실제로 수정할 때 어떤 코드 구조로 바꾸면 좋을지”까지 이어서 정리해드릴 수 있습니다.


------

- 아래는 테스트 케이스 관련 문서


I’m organizing the validation steps into a concise test plan focused on the two behaviors you care about: hierarchy-state updates and behaviour event hookup/unhooking.

네, 그 두 가지만 확인하면 충분합니다.

## 테스트 목표
1. `ActiveInHierarchy`가 부모/자식 상태 변화에 따라 올바르게 갱신되는지 확인한다.
2. `Behaviour`가 하이어라키 변경 이벤트를 제대로 구독하고, 파괴 시점에 해제되는지 확인한다.

---

## 테스트 케이스 1: ActiveInHierarchy 기본 동작

### 1-1. 루트 GameObject가 비활성화되면 자식도 비활성화된다
- 루트 `GameObject`를 생성한다.
- 그 아래에 자식 `GameObject`를 하나 생성한다.
- 자식의 `ActiveInHierarchy` 값을 확인한다.
- 루트의 `ActiveSelf`를 `false`로 바꾼다.
- 자식의 `ActiveInHierarchy` 값이 `false`로 바뀌는지 확인한다.

예상 결과:
- 자식은 루트가 비활성화되면 `ActiveInHierarchy == false`가 되어야 한다.

### 1-2. 루트가 다시 활성화되면 자식도 활성화된다
- 위 상태에서 루트의 `ActiveSelf`를 `true`로 바꾼다.
- 자식의 `ActiveInHierarchy` 값이 다시 `true`가 되는지 확인한다.

예상 결과:
- 자식은 루트가 다시 활성화되면 `ActiveInHierarchy == true`가 되어야 한다.

### 1-3. 자식 자체가 비활성화되면 하위 트리 상태도 반영된다
- 자식의 `ActiveSelf`를 `false`로 바꾼다.
- 자식의 `ActiveInHierarchy`가 `false`가 되는지 확인한다.

예상 결과:
- 자식 자신은 비활성화 상태가 되어야 한다.

### 1-4. 부모-자식 관계 변경 시 상태가 갱신된다
- 자식이 어떤 부모 아래에 있는 상태에서 다른 부모로 이동한다.
- 이동 후 `ActiveInHierarchy`가 새 부모의 상태를 따라가는지 확인한다.

예상 결과:
- 부모가 바뀌면 `ActiveInHierarchy` 계산 결과도 바뀌어야 한다.

---

## 테스트 케이스 2: Behaviour 하이어라키 이벤트 후킹

### 2-1. Behaviour가 등록되면 하이어라키 변경 이벤트를 받는다
- `Behaviour`를 하나 가진 `GameObject`를 만든다.
- 그 `Behaviour`가 등록된 이후, 부모 또는 자기 자신의 활성 상태를 바꾼다.
- `BehaviourSystem`의 상태 변경 처리로 인해 해당 `Behaviour`가 반응하는지 확인한다.

예상 결과:
- 하이어라키 상태 변화가 발생하면 해당 `Behaviour`가 `CheckChangeState` 처리 대상에 들어가야 한다.

### 2-2. 부모 활성 상태 변화가 반영된다
- `Behaviour`가 붙은 자식 `GameObject`를 만든다.
- 부모 `GameObject`를 비활성화한다.
- 해당 `Behaviour`가 비활성 상태 처리 대상으로 반영되는지 확인한다.

예상 결과:
- 부모가 비활성화되면 `Behaviour`는 비활성 상태로 바뀌어야 한다.

### 2-3. 자식 자체가 비활성화되면 반영된다
- 자식 `GameObject`의 `ActiveSelf`를 `false`로 바꾼다.
- 해당 `Behaviour`의 활성 상태가 반영되는지 확인한다.

예상 결과:
- 자식이 비활성화되면 해당 `Behaviour`도 비활성 상태로 처리되어야 한다.

### 2-4. 파괴 시 이벤트 해제
- `Behaviour`가 붙은 `GameObject`를 파괴한다.
- 파괴 이후 해당 `Behaviour`가 더 이상 하이어라키 변화 이벤트를 받지 않는지 확인한다.

예상 결과:
- 파괴된 객체의 `Behaviour`는 더 이상 이벤트 구독 대상이 아니어야 한다.
- 메모리/상태 추적상 문제 없이 정리되어야 한다.

---

## 추가 권장 포인트
- 위 테스트는 “단일 객체”, “부모-자식 관계”, “파괴” 세 가지 시나리오로 나누면 보기 좋습니다.
- 특히 `ActiveInHierarchy`는 “부모 상태 변화”와 “자기 상태 변화”가 서로 다른 경로이므로 둘 다 체크하는 것이 좋습니다.
- `Behaviour` 쪽은 “구독 등록 → 상태 변화 → 파괴 시 해제” 흐름이 핵심입니다.

원하면 다음으로는 이 테스트 케이스를 실제 코드 레벨에서 바로 실행 가능한 형태로 정리해서, 예시 코드 스니펫 형태로 이어서 적어드릴 수 있습니다.