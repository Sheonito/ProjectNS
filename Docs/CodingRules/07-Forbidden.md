% Forbidden

- `sealed` 금지
- `??` null 병합 연산자 금지
- `var` 금지(타입이 과도하게 긴 경우에만 예외)
- **쌍방참조(양방향 참조) 금지**: A → B 참조 시 B → A 참조 불가. EventBus를 통해 통신해야 함
- private 변수를 public 프로퍼티로 노출 금지 (필요시 메서드 또는 이벤트 사용)
- `UniTaskCompletionSource` 금지

## 쌍방참조 금지 상세

쌍방참조는 메모리 관리, 테스트, 확장성 측면에서 문제를 일으킵니다.

### 금지 예시
```csharp
// ❌ 금지: Player → Container → Player (쌍방참조)
public class Player
{
    private PlayerContainer _container; // Container가 Player를 참조
}

public class PlayerContainer
{
    public Player Player { get; } // Player가 Container를 참조
}
```

### 허용 예시
```csharp
// ✅ 허용: EventBus를 통한 통신
public class Player
{
    private void OnSomeEvent()
    {
        EventBus.Publish(this, new SomeEvent());
    }
}

public class SomeState
{
    public void Enter()
    {
        EventBus.Subscribe<SomeEvent>(OnSomeEvent);
    }
}
```

## private 변수 노출 금지 상세

private 변수를 public 프로퍼티로 노출하는 것은 캡슐화를 깨뜨립니다.

### 금지 예시
```csharp
// ❌ 금지: private 변수를 public으로 노출
private PlayerMovement _movement;
public PlayerMovement Movement => _movement;
```

### 허용 예시
```csharp
// ✅ 허용: 필요한 기능만 메서드로 제공
private PlayerMovement _movement;

public void SetHorizontalInput(float input)
{
    _movement.SetHorizontalInput(input);
}

// ✅ 또는 이벤트를 통해 통신
EventBus.Publish(this, new MovementRequestEvent(input));
```