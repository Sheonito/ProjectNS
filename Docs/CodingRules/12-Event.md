% EventBus

이 문서는 프로젝트의 EventBus 이벤트 시스템 사용 규칙을 정의합니다.

## 기본 구조
- `EventBus`는 정적 클래스로 전역 이벤트 시스템 제공
- `IEvent` 인터페이스를 구현하여 이벤트 클래스 작성
- `Subscribe`/`Unsubscribe`로 구독, `Publish`로 발행

## 네임스페이스
- `Waving.Common.Event`: EventBus 및 IEvent 인터페이스
- 이벤트 구현은 기능별 네임스페이스 사용 (예: `Waving.BlackSpin.Event`)

## 이벤트 클래스 구현 규칙
- `IEvent` 인터페이스 구현 필수
- `GetPublishType()`: 이 이벤트를 발행할 수 있는 타입 반환 (발행자 타입 제한)
- 이벤트는 불변(immutable) 데이터 클래스로 구현
- 프로퍼티는 private set으로 외부 수정 방지

## 구독 규칙
- `EventBus.Subscribe<TEvent>(Action<TEvent> listener)`로 구독
- 구독은 보통 씬 진입 시 `OnEnter()`에서 등록
- 씬 종료 시 `OnExit()`에서 `Unsubscribe`로 해제 필수 (메모리 누수 방지)
- 동일한 리스너 중복 구독은 자동으로 무시됨

## 발행 규칙
- `EventBus.Publish<TEvent>(object owner, TEvent payload)` 사용
- `owner`는 `GetPublishType()`에서 반환한 타입과 일치해야 함 (타입 안전성)
- `owner`가 null이면 예외 발생
- 발행은 즉시 모든 구독자에게 전달됨 (동기 실행)

## 발행 오버로드
- `Publish<TEvent>(object owner)`: 기본 생성자로 이벤트 생성
- `Publish<TEvent>(object owner, TEvent payload)`: 기존 인스턴스 전달
- `Publish<TEvent>(object owner, Func<TEvent> eventAction)`: 팩토리 함수로 생성

## 사용 예시

### 이벤트 클래스 구현
```csharp
public class SampleEvent : IEvent
{
    public int Value { get; private set; }
    
    public SampleEvent(int value)
    {
        Value = value;
    }
    
    public Type GetPublishType()
    {
        return typeof(SampleState);
    }
}
```

### 이벤트 구독 및 해제
```csharp
public class SampleState : IState
{
    public void Enter()
    {
        EventBus.Subscribe<SampleEvent>(OnSampleEvent);
    }
    
    public void Exit()
    {
        EventBus.Unsubscribe<SampleEvent>(OnSampleEvent);
    }
    
    private void OnSampleEvent(SampleEvent evt)
    {
        Debug.Log($"Event received: {evt.Value}");
    }
}
```

### 이벤트 발행
```csharp
public class SampleState : IState
{
    public void Execute()
    {
        if (someCondition)
        {
            EventBus.Publish(this, new SampleEvent(100));
        }
    }
}
```

## 금지 사항
- 구독 해제 누락 금지 (메모리 누수 원인)
- 이벤트 내부에서 상태 변경 금지 (데이터 전달만)
- 순환 이벤트 발행 금지 (무한 루프 위험)
- `GetPublishType()`을 잘못 구현하여 타입 불일치 발생 금지
