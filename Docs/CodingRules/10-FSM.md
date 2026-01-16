% FSM (Finite State Machine)

이 문서는 프로젝트의 FSM(Finite State Machine) 시스템 사용 규칙을 정의합니다.

## 기본 구조
- `StateMachine`을 상속하여 상태 머신 구현
- `IState` 인터페이스를 구현하여 상태 클래스 작성
- 상태 전환은 `StateTransition`을 통해 조건 기반으로 처리

## 네임스페이스
- `StateMachine.Runtime`: StateMachine 및 관련 클래스
- `SRPG`: IState 인터페이스
- 상태 구현은 기능별 네임스페이스 사용 (예: `Waving.BlackSpin.FSM`)

## 상태 머신 구현 규칙
- `Init(IState state)`에서 초기 상태 설정 및 `Enter()` 호출
- `Execute()`는 Unity `Update()`에서 매 프레임 호출
- 상태 전환은 `ChangeState(IState newState)` 사용
- 이전 상태로 돌아가려면 `BackToPreState()` 사용
- 전역 상태가 필요하면 `SetGlobalState(IState newState)` 사용

## 상태 클래스 구현 규칙
- `IState` 인터페이스 구현 필수
- `Enter()`: 상태 진입 시 1회 호출, 초기화 로직
- `Execute()`: 매 프레임 호출, 업데이트 로직
- `Exit()`: 상태 종료 시 1회 호출, 정리 로직
- 상태 내부에서 DI를 사용할 경우 생성자에서 `DIResolver.Resolve<T>()` 호출

## 상태 전환 규칙
- `AddTransition(StateTransition transition)`로 전환 조건 등록
- `StateTransition`의 `condition`은 `Func<bool>` 타입
- 조건이 true일 때 `nextState`로 전환
- 전환 조건은 `Execute()`에서 자동으로 평가됨

## 금지 사항
- `ChangeState()`를 직접 호출하지 말고 `StateTransition` 사용 권장
- 상태 내부에서 다른 상태의 내부 로직 직접 호출 금지
- 상태 간 직접 참조 지양, 이벤트나 전역 상태 활용
