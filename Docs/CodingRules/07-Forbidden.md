% Forbidden

- `sealed` 금지
- `??` null 병합 연산자 금지
- `var` 금지(타입이 과도하게 긴 경우에만 예외)
- **쌍방참조(양방향 참조) 금지**: A → B 참조 시 B → A 참조 불가. EventBus를 통해 통신해야 함
- private 변수를 public 프로퍼티로 노출 금지 (필요시 메서드 또는 이벤트 사용)
- `UniTaskCompletionSource` 금지
- **Coroutine 금지**: 비동기 처리는 무조건 UniTask 사용
- **빈 이벤트(구독자 없는 이벤트) 금지**: 발행만 하고 구독하지 않는 이벤트 생성 금지

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

## 불필요한 null 처리 금지 상세

생성자에서 초기화되거나 항상 유효한 참조에 대한 불필요한 null 체크는 코드를 복잡하게 만들고 가독성을 해칩니다.

### 금지 예시
```csharp
// ❌ 금지: 생성자에서 초기화된 readonly 필드에 대한 null 체크
public class PlayerAnimator
{
    private readonly Animator _animator;

    public PlayerAnimator(Animator animator)
    {
        _animator = animator; // 생성자에서 초기화
    }

    public void PlayAnimation(string name)
    {
        if (_animator == null) // 불필요한 null 체크
        {
            Debug.LogWarning("Animator is null");
            return;
        }
        _animator.Play(name);
    }
}
```

## Coroutine 금지 상세

모든 비동기 처리는 UniTask를 사용합니다. Coroutine은 취소 처리가 어렵고 반환값 처리가 불편합니다.

### 금지 예시
```csharp
// ❌ 금지: Coroutine 사용
private IEnumerator FadeOut()
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }
}

// 호출
StartCoroutine(FadeOut());
```

### 올바른 사용법
```csharp
// ✅ 허용: UniTask 사용 (복잡한 비동기 로직)
private async UniTaskVoid LoadDataAsync()
{
    CancellationToken token = _cts.Token;
    await someAsyncOperation(token);
}

// 호출
LoadDataAsync().Forget();
```

## DOTween 사용 규칙

Fade, 이동, 스케일 등 간단한 트윈 애니메이션은 DOTween을 사용합니다.

### 금지 예시
```csharp
// ❌ 금지: Fade를 UniTask/Coroutine으로 구현
private async UniTaskVoid FadeOutAsync()
{
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
        _sprite.color = new Color(1f, 1f, 1f, alpha);
        await UniTask.Yield();
    }
}
```

### 올바른 사용법
```csharp
// ✅ 허용: DOTween 사용
private void FadeOut()
{
    _sprite.DOFade(0f, duration)
        .SetEase(Ease.OutQuad)
        .OnComplete(OnFadeComplete);
}

// ✅ 허용: 여러 트윈을 동시에 실행
private void FadeOutGroup()
{
    Sequence sequence = DOTween.Sequence();
    foreach (SpriteRenderer sprite in _spriteGroup)
    {
        sequence.Join(sprite.DOFade(0f, duration));
    }
    sequence.SetEase(Ease.OutQuad)
            .OnComplete(OnFadeComplete);
}
```

## 빈 이벤트(구독자 없는 이벤트) 금지 상세

Publish만 하고 Subscribe하지 않는 이벤트는 불필요한 코드입니다. 이벤트를 생성하기 전에 실제로 구독하는 곳이 있는지 확인해야 합니다.

### 금지 예시
```csharp
// ❌ 금지: 발행만 하고 구독하지 않는 이벤트
public class PlayerDamageEvent : IEvent { ... }

// Player.cs에서 발행
EventBus.Publish(this, new PlayerDamageEvent(damage, hp));

// 하지만 어디에서도 Subscribe하지 않음
// EventBus.Subscribe<PlayerDamageEvent>(handler); // 없음
```

### 올바른 접근
```csharp
// ✅ 이벤트 없이 직접 상태 전환
public void OnDamaged(int damage)
{
    ApplyDamage(damage);

    if (IsDead)
    {
        EventBus.Publish(this, new PlayerChangeStateRequestEvent(PlayerStateType.Death));
    }
    else
    {
        EventBus.Publish(this, new PlayerChangeStateRequestEvent(PlayerStateType.Damaged));
    }
}
```