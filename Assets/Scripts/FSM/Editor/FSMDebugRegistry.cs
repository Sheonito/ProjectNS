using System;
using System.Collections.Generic;
using UnityEditor;

namespace Percent111.ProjectNS.FSM.Editor
{
    // 상태 변경 히스토리 데이터
    public class StateChangeRecord
    {
        public DateTime Timestamp;
        public string FromState;
        public string ToState;
    }
    
    // 개별 FSM의 디버그 데이터
    public class FSMDebugData
    {
        public StateMachine Instance;
        public string Name;
        public List<StateChangeRecord> History;
        public int MaxHistoryCount;
        
        public FSMDebugData(StateMachine instance, int maxHistoryCount = 100)
        {
            Instance = instance;
            Name = instance.Name;
            History = new List<StateChangeRecord>();
            MaxHistoryCount = maxHistoryCount;
        }
        
        // 히스토리 추가
        public void AddHistory(IState fromState, IState toState)
        {
            StateChangeRecord record = new StateChangeRecord
            {
                Timestamp = DateTime.Now,
                FromState = fromState?.GetType().Name ?? "(null)",
                ToState = toState?.GetType().Name ?? "(null)"
            };
            
            History.Add(record);
            
            // 최대 개수 초과 시 오래된 기록 삭제
            if (History.Count > MaxHistoryCount)
            {
                History.RemoveAt(0);
            }
        }
        
        // 히스토리 초기화
        public void ClearHistory()
        {
            History.Clear();
        }
    }
    
    [InitializeOnLoad]
    public static class FSMDebugRegistry
    {
        private static Dictionary<StateMachine, FSMDebugData> _debugDataMap;
        
        public static event Action OnRegistryChanged;
        
        public static int MaxHistoryCount { get; set; } = 100;
        
        static FSMDebugRegistry()
        {
            _debugDataMap = new Dictionary<StateMachine, FSMDebugData>();
            
            StateMachine.OnInstanceCreated += OnInstanceCreated;
            StateMachine.OnInstanceDestroyed += OnInstanceDestroyed;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        // FSM 인스턴스 생성 시 호출
        private static void OnInstanceCreated(StateMachine instance)
        {
            if (instance == null || _debugDataMap.ContainsKey(instance))
                return;
            
            FSMDebugData debugData = new FSMDebugData(instance, MaxHistoryCount);
            _debugDataMap.Add(instance, debugData);
            
            instance.OnStateChanged += (from, to) => OnStateChanged(instance, from, to);
            
            OnRegistryChanged?.Invoke();
        }
        
        // FSM 인스턴스 파괴 시 호출
        private static void OnInstanceDestroyed(StateMachine instance)
        {
            if (instance == null || !_debugDataMap.ContainsKey(instance))
                return;
            
            _debugDataMap.Remove(instance);
            
            OnRegistryChanged?.Invoke();
        }
        
        // 상태 변경 시 호출
        private static void OnStateChanged(StateMachine instance, IState fromState, IState toState)
        {
            if (!_debugDataMap.TryGetValue(instance, out FSMDebugData debugData))
                return;
            
            debugData.AddHistory(fromState, toState);
            
            OnRegistryChanged?.Invoke();
        }
        
        // 플레이 모드 변경 시 데이터 초기화
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _debugDataMap.Clear();
                OnRegistryChanged?.Invoke();
            }
        }
        
        // 등록된 모든 FSM 디버그 데이터 반환
        public static IReadOnlyCollection<FSMDebugData> GetAllDebugData()
        {
            return _debugDataMap.Values;
        }
        
        // 특정 FSM의 디버그 데이터 반환
        public static FSMDebugData GetDebugData(StateMachine instance)
        {
            _debugDataMap.TryGetValue(instance, out FSMDebugData debugData);
            return debugData;
        }
        
        // 등록된 FSM 개수 반환
        public static int GetRegisteredCount()
        {
            return _debugDataMap.Count;
        }
        
        // 모든 히스토리 초기화
        public static void ClearAllHistory()
        {
            foreach (FSMDebugData debugData in _debugDataMap.Values)
            {
                debugData.ClearHistory();
            }
            
            OnRegistryChanged?.Invoke();
        }
    }
}
