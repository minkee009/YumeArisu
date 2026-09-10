# VS Code 개발 환경 셋업

> 버전 : 0.1  
> 베이스 : .NET 9.0+  
> 마지막 업데이트 : 2026-09-10  
> 작성자 : minkee009

이 프로젝트를 처음 클론했다면 아래 순서대로 셋업하세요.

## 1. dotnet 템플릿 설치 (최초 1회, 또는 `Templates/` 폴더 수정 시마다)

`Ctrl+Shift+P` → `Tasks: Run Task` → **`Setup: Install Templates`** 실행

또는 터미널에서 직접:

```bash
dotnet new install ./Templates --force
```

## 2. 새 Scene 생성하기
템플릿 설치 이후 아래와 같은 방법으로 빠르게 새 Scene 소스파일을 만들 수 있습니다.
1. `Ctrl+Shift+P` → `Tasks: Run Task` → `New Scene` 실행
2. 씬 클래스 이름 작성 (예: `TestScene`)
3. 출력 폴더 경로 입력 (기본값: `YumeArisu.Game/src/Scenes`)

`{입력한 이름}.cs` 파일이 지정 폴더에 생성되고, 클래스명도 자동으로 반영됩니다.

## 3. 새 ScriptBehaviour 생성하기

또한 아래와 같은 방법으로 빠르게 새 ScriptBehaviour를 작성할 수 있습니다. 

1. `Ctrl+Shift+P` → `Tasks: Run Task` → `New ScriptBehaviour` 실행
2. 클래스 이름 입력 (예: `TestChatter`)
3. 출력 폴더 경로 입력 (기본값: `YumeArisu.Game/src/Scripts`)

마찬가지로 `{입력한 이름}.cs` 파일이 지정 폴더에 생성되고, 클래스명도 자동으로 반영됩니다.

## 5. 그외에 리소스 템플릿으로 새 리소스파일(json) 생성하기

위 방법들과 마찬가지로 템플릿 작업을 통해 Sprite,Material,Mesh,Shader,... 등 다양한 리소스파일을 생성할 수 있습니다.

## 6. 템플릿 수정 시 주의사항

`Templates/ScriptBehaviour/NewBehaviour.cs`를 수정한 뒤에는, 반드시 `Setup: Install Templates`를 다시 실행해서 변경사항을 반영해야 합니다. `dotnet new`는 설치 시점의 스냅샷을 사용하기 때문에, 파일만 고치고 재설치를 안 하면 이전 버전이 계속 생성됩니다. 그럼에도 반영이 되지 않는다면 `Setup: Reinit Templates` 작업을 실행해 캐시를 완전히 제거한 뒤 다시 시도해보세요

## 참고: 관련 설정 파일 위치

| 파일 | 역할 |
|---|---|
| `.vscode/tasks.json` | 태스크 정의 (템플릿 설치, 스크립트 생성) |
| `Templates/**/` | `dotnet new` 커스텀 템플릿 원본 |
