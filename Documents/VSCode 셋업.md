# VS Code 개발 환경 셋업

이 프로젝트를 처음 클론했다면 아래 순서대로 셋업하세요.

## 1. 권장 확장 설치

`.code-workspace`를 열면 VS Code가 권장 확장 목록을 알려줍니다. 알림이 뜨면 **Install All**을 눌러주세요.

수동으로 설치할 경우 아래 확장이 필요합니다.

- [Task Buttons](https://marketplace.visualstudio.com/items?itemName=spencerwmiles.vscode-task-buttons) — 상태바에서 커스텀 태스크를 버튼으로 실행

## 2. dotnet 템플릿 설치 (최초 1회, 또는 `Templates/` 폴더 수정 시마다)

`Ctrl+Shift+P` → `Tasks: Run Task` → **`Setup: Install Templates`** 실행

또는 터미널에서 직접:

```bash
dotnet new install ./Templates/ScriptBehaviour --force
```

## 3. 새 ScriptBehaviour 생성하기

설치가 끝나면 상태바에 **New ScriptBehaviour** 버튼이 나타납니다.

1. 상태바 버튼 클릭 (또는 `Ctrl+Shift+P` → `Tasks: Run Task` → `New ScriptBehaviour`)
2. 클래스 이름 입력 (예: `TestChatter`)
3. 출력 폴더 경로 입력 (기본값: `YumeArisu.Game/src/Scripts`)

`{입력한 이름}.cs` 파일이 지정 폴더에 생성되고, 클래스명도 자동으로 반영됩니다.

## 4. 템플릿 수정 시 주의사항

`Templates/ScriptBehaviour/NewBehaviour.cs`를 수정한 뒤에는, 반드시 **`Setup: Install Templates`**를 다시 실행해서 변경사항을 반영해야 합니다. `dotnet new`는 설치 시점의 스냅샷을 사용하기 때문에, 파일만 고치고 재설치를 안 하면 이전 버전이 계속 생성됩니다.

## 참고: 관련 설정 파일 위치

| 파일 | 역할 |
|---|---|
| `.vscode/tasks.json` | 태스크 정의 (템플릿 설치, 스크립트 생성) |
| `YumeArisu.code-workspace` | Task Buttons 설정 (`settings.VsCodeTaskButtons.tasks`) |
| `Templates/ScriptBehaviour/` | `dotnet new` 커스텀 템플릿 원본 |