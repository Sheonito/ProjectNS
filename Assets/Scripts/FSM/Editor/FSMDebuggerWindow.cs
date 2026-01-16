using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Waving.BlackSpin.FSM.Editor
{
    public class FSMDebuggerWindow : EditorWindow
    {
        private const string ALL_FSM_OPTION = "[ 모든 FSM ]";
        
        private VisualElement _root;
        private DropdownField _fsmDropdown;
        private VisualElement _stateInfoSection;
        private Label _currentStateLabel;
        private Label _previousStateLabel;
        private Label _globalStateLabel;
        private ScrollView _historyScrollView;
        private Label _emptyMessageLabel;
        
        private FSMDebugData _selectedDebugData;
        private List<FSMDebugData> _debugDataList;
        private bool _isAllFSMSelected;
        
        [MenuItem("Tools/FSM Debugger")]
        public static void ShowWindow()
        {
            FSMDebuggerWindow window = GetWindow<FSMDebuggerWindow>();
            window.titleContent = new GUIContent("FSM Debugger");
            window.minSize = new Vector2(400, 500);
        }
        
        private void OnEnable()
        {
            _debugDataList = new List<FSMDebugData>();
            
            FSMDebugRegistry.OnRegistryChanged += OnRegistryChanged;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            FSMDebugRegistry.OnRegistryChanged -= OnRegistryChanged;
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void CreateGUI()
        {
            _root = rootVisualElement;
            
            // UXML 로드
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Scripts/FSM/Editor/FSMDebuggerWindow.uxml");
            
            if (visualTree != null)
            {
                visualTree.CloneTree(_root);
            }
            else
            {
                CreateFallbackUI();
                return;
            }
            
            // USS 로드
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Scripts/FSM/Editor/FSMDebuggerWindow.uss");
            
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }
            
            BindUIElements();
            RegisterEvents();
            RefreshFSMList();
        }
        
        // UXML 파일이 없을 경우 기본 UI 생성
        private void CreateFallbackUI()
        {
            _root.Add(new Label("UXML 파일을 찾을 수 없습니다."));
            _root.Add(new Label("Assets/Scripts/FSM/Editor/FSMDebuggerWindow.uxml"));
        }
        
        // UI 요소 바인딩
        private void BindUIElements()
        {
            _fsmDropdown = _root.Q<DropdownField>("fsm-dropdown");
            _stateInfoSection = _root.Q<VisualElement>("state-info");
            _currentStateLabel = _root.Q<Label>("current-state-value");
            _previousStateLabel = _root.Q<Label>("previous-state-value");
            _globalStateLabel = _root.Q<Label>("global-state-value");
            _historyScrollView = _root.Q<ScrollView>("history-scroll-view");
            _emptyMessageLabel = _root.Q<Label>("empty-message");
            
            Button clearHistoryButton = _root.Q<Button>("clear-history-button");
            if (clearHistoryButton != null)
            {
                clearHistoryButton.clicked += OnClearHistoryClicked;
            }
        }
        
        // 이벤트 등록
        private void RegisterEvents()
        {
            if (_fsmDropdown != null)
            {
                _fsmDropdown.RegisterValueChangedCallback(OnFSMSelectionChanged);
            }
        }
        
        // FSM 선택 변경 시 호출
        private void OnFSMSelectionChanged(ChangeEvent<string> evt)
        {
            string selectedName = evt.newValue;
            _isAllFSMSelected = selectedName == ALL_FSM_OPTION;
            
            if (_isAllFSMSelected)
            {
                _selectedDebugData = null;
            }
            else
            {
                _selectedDebugData = _debugDataList.FirstOrDefault(data => data.Name == selectedName);
            }
            
            UpdateStateInfoVisibility();
            RefreshStateInfo();
            RefreshHistory();
        }
        
        // 상태 정보 섹션 표시/숨김
        private void UpdateStateInfoVisibility()
        {
            if (_stateInfoSection == null)
                return;
            
            _stateInfoSection.style.display = _isAllFSMSelected ? DisplayStyle.None : DisplayStyle.Flex;
        }
        
        // 히스토리 초기화 버튼 클릭 시 호출
        private void OnClearHistoryClicked()
        {
            if (_isAllFSMSelected)
            {
                FSMDebugRegistry.ClearAllHistory();
            }
            else if (_selectedDebugData != null)
            {
                _selectedDebugData.ClearHistory();
            }
            
            RefreshHistory();
        }
        
        // Registry 변경 시 호출
        private void OnRegistryChanged()
        {
            RefreshFSMList();
        }
        
        // 에디터 업데이트 시 호출 (상태 정보 갱신)
        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;
            
            RefreshStateInfo();
        }
        
        // FSM 목록 갱신
        private void RefreshFSMList()
        {
            _debugDataList = FSMDebugRegistry.GetAllDebugData().ToList();
            
            if (_fsmDropdown == null)
                return;
            
            List<string> choices = new List<string>();
            
            // FSM이 2개 이상일 때만 "모든 FSM" 옵션 표시
            if (_debugDataList.Count >= 2)
            {
                choices.Add(ALL_FSM_OPTION);
            }
            
            choices.AddRange(_debugDataList.Select(data => data.Name));
            _fsmDropdown.choices = choices;
            
            // 이전 선택 유지 또는 첫 번째 항목 선택
            if (_isAllFSMSelected && choices.Contains(ALL_FSM_OPTION))
            {
                _fsmDropdown.value = ALL_FSM_OPTION;
            }
            else if (_selectedDebugData != null && choices.Contains(_selectedDebugData.Name))
            {
                _fsmDropdown.value = _selectedDebugData.Name;
            }
            else if (_debugDataList.Count > 0)
            {
                _isAllFSMSelected = false;
                _fsmDropdown.value = _debugDataList[0].Name;
                _selectedDebugData = _debugDataList[0];
            }
            else
            {
                _isAllFSMSelected = false;
                _fsmDropdown.value = null;
                _selectedDebugData = null;
            }
            
            UpdateEmptyMessage();
            UpdateStateInfoVisibility();
            RefreshStateInfo();
            RefreshHistory();
        }
        
        // 빈 메시지 표시 업데이트
        private void UpdateEmptyMessage()
        {
            if (_emptyMessageLabel == null)
                return;
            
            bool isEmpty = _debugDataList.Count == 0;
            _emptyMessageLabel.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        // 상태 정보 갱신
        private void RefreshStateInfo()
        {
            if (_selectedDebugData == null || _selectedDebugData.Instance == null)
            {
                SetStateLabels("(없음)", "(없음)", "(없음)");
                return;
            }
            
            StateMachine.Runtime.StateMachine instance = _selectedDebugData.Instance;
            
            string currentState = instance.CurState?.GetType().Name ?? "(없음)";
            string previousState = instance.PreState?.GetType().Name ?? "(없음)";
            string globalState = instance.GlobalState?.GetType().Name ?? "(없음)";
            
            SetStateLabels(currentState, previousState, globalState);
        }
        
        // 상태 라벨 설정
        private void SetStateLabels(string current, string previous, string global)
        {
            if (_currentStateLabel != null)
                _currentStateLabel.text = current;
            
            if (_previousStateLabel != null)
                _previousStateLabel.text = previous;
            
            if (_globalStateLabel != null)
                _globalStateLabel.text = global;
        }
        
        // 히스토리 목록 갱신
        private void RefreshHistory()
        {
            if (_historyScrollView == null)
                return;
            
            _historyScrollView.Clear();
            
            if (_isAllFSMSelected)
            {
                RefreshAllFSMHistory();
                return;
            }
            
            if (_selectedDebugData == null || _selectedDebugData.History.Count == 0)
            {
                Label emptyLabel = new Label("히스토리가 없습니다.");
                emptyLabel.AddToClassList("history-empty");
                _historyScrollView.Add(emptyLabel);
                return;
            }
            
            // 최신 기록이 위에 오도록 역순으로 표시
            for (int i = _selectedDebugData.History.Count - 1; i >= 0; i--)
            {
                StateChangeRecord record = _selectedDebugData.History[i];
                VisualElement historyItem = CreateHistoryItem(record, null);
                _historyScrollView.Add(historyItem);
            }
        }
        
        // 모든 FSM 히스토리 표시
        private void RefreshAllFSMHistory()
        {
            // 모든 FSM의 히스토리를 합쳐서 시간순 정렬
            List<(FSMDebugData data, StateChangeRecord record)> allRecords = new List<(FSMDebugData, StateChangeRecord)>();
            
            foreach (FSMDebugData debugData in _debugDataList)
            {
                foreach (StateChangeRecord record in debugData.History)
                {
                    allRecords.Add((debugData, record));
                }
            }
            
            if (allRecords.Count == 0)
            {
                Label emptyLabel = new Label("히스토리가 없습니다.");
                emptyLabel.AddToClassList("history-empty");
                _historyScrollView.Add(emptyLabel);
                return;
            }
            
            // 시간 역순 정렬 (최신이 위)
            allRecords.Sort((a, b) => b.record.Timestamp.CompareTo(a.record.Timestamp));
            
            foreach ((FSMDebugData data, StateChangeRecord record) item in allRecords)
            {
                VisualElement historyItem = CreateHistoryItem(item.record, item.data.Name);
                _historyScrollView.Add(historyItem);
            }
        }
        
        // 히스토리 아이템 생성
        private VisualElement CreateHistoryItem(StateChangeRecord record, string fsmName)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("history-item");
            
            Label timestampLabel = new Label(record.Timestamp.ToString("HH:mm:ss.fff"));
            timestampLabel.AddToClassList("history-timestamp");
            container.Add(timestampLabel);
            
            // FSM 이름 표시 (모든 FSM 보기일 때)
            if (fsmName != null)
            {
                Label fsmNameLabel = new Label($"[{fsmName}]");
                fsmNameLabel.AddToClassList("history-fsm-name");
                container.Add(fsmNameLabel);
            }
            
            Label transitionLabel = new Label($"{record.FromState} → {record.ToState}");
            transitionLabel.AddToClassList("history-transition");
            container.Add(transitionLabel);
            
            return container;
        }
    }
}
