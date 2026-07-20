# 코드 작성법

> 버전 : 0.3  
> 베이스 : .NET 9.0  
> 마지막 업데이트 : 2026-07-20  
> 작성자 : minkee009

## .NET 프로젝트 일관성을 우선시
- .NET 9.0 기반의 코드 컨벤션이 우선적인 원칙입니다.
- 일부 예외는 하단부터 서술합니다.

## 코드 블럭 혹은 스코프 작성 법

### `if-else`, `else if`

```CSharp
if (condition)
    INeedNicotine();
```
- 분기 밑 실행 코드가 한 줄인 경우 코드 블럭은 생략합니다. 두 줄 이상부터는 코드 블럭을 사용합니다.

&nbsp; 
```CSharp
if (miyako)
    PPyong();
else if (miyoo)
    Heueung();
else
    PPang();
```
- if-else, if-else_if-else, ... 등의 체인이 있는 경우 모든 분기에서 한 줄 처리가 가능할 때만 생략합니다.

&nbsp; 
```CSharp
if (boolean_a)
{
    FuncA();
    FuncB();
}
else
{
    FuncC();
}
```
- 한 분기라도 두 줄 이상인 경우 코드 블럭을 꼭 사용합시다.

&nbsp; 
```CSharp
if (OhMyGA)
{
    if(what)
        Hello();
}
```
- 내부 분기의 생략문의 경우 두 줄로 봅니다.

### `while`, `for`, `foreach`

```CSharp
while(!quit)
    Spamming();

foreach(var go in GameObjects)
{
    go.ActiveSelf = true;
    go.Name = $"Enemy[{enemyCount++}]";
}
```
- 반복문도 비슷한 기조를 따라갑니다.
- 두 줄 이상인 경우엔 꼭 코드 블럭을 사용합시다.

&nbsp;
```CSharp
foreach (var comp in Components)
{
    if (comp is Attacker attacker)
        attacker.Attack();
}
```
-  마찬가지로 반복문 내부의 생략문은 두 줄로 봅니다.

## 함수 작성 법
### 생략
```CSharp
void SayAnything() => Console.WriteLine("아하하");
```
- 한 줄 작성 가능한 것은 `=>` 구문으로 생략합니다.

### 명시적으로 구현하지 않음을 표시
```CSharp
public virtual void OnEnable() { }
```
- 하위 클래스가 구현을 빼먹어도 상관없는 함수, 즉 구현해도 되고 하지 않아도 되는 것을 명시해 두기 위해 다음과 같이 함수 바로 오른편에 코드 블럭을 짧게 배치합니다.

### 구현 예정
```CSharp
public void JustDoIt()
{
    // TODO : 뭘 어떻게 해야하긴 함
}
```
- 코드 블럭을 전개한 상태의 공백 처리는 기본적으로 `구현 예정`으로 간주합니다.
- 작업자간 코드 이해 차이로 인해 생기는 병목을 줄이고자 `TODO 주석`을 달아주는 것을 권장합니다.

## 프로퍼티 작성 법
작성 예정