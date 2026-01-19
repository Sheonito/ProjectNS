using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.Event;
using TMPro;
using UnityEngine;

namespace Percent111.ProjectNS.UI
{
    // 스테이지 정보 UI (타이머 + 스테이지 번호)
    public class StageUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Timer Format")]
        [SerializeField] private string _timerFormat = "{0:00}:{1:00}";
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _warningThreshold = 10f;

        private StageManager _stageManager;

        // 초기화 (BattleManager에서 호출)
        public void Initialize(StageManager stageManager)
        {
            _stageManager = stageManager;

            // EventBus 구독
            EventBus.Subscribe<StageStartedEvent>(OnStageStarted);
            EventBus.Subscribe<StageTimerUpdatedEvent>(OnTimerUpdated);

            // 초기 표시
            UpdateStageText(_stageManager.CurrentStage);
            UpdateTimerText(_stageManager.RemainingTime);
        }

        // 스테이지 시작 이벤트 핸들러
        private void OnStageStarted(StageStartedEvent evt)
        {
            UpdateStageText(evt.StageNumber);
        }

        // 타이머 업데이트 이벤트 핸들러
        private void OnTimerUpdated(StageTimerUpdatedEvent evt)
        {
            UpdateTimerText(evt.RemainingTime);
        }

        // 스테이지 텍스트 업데이트
        private void UpdateStageText(int stageNumber)
        {
            if (_stageText != null)
            {
                _stageText.text = $"STAGE {stageNumber}";
            }
        }

        // 타이머 텍스트 업데이트
        private void UpdateTimerText(float remainingTime)
        {
            if (_timerText == null) return;

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            _timerText.text = string.Format(_timerFormat, minutes, seconds);

            // 경고 색상 적용
            _timerText.color = remainingTime <= _warningThreshold ? _warningColor : _normalColor;
        }

        // 정리
        private void OnDestroy()
        {
            EventBus.Unsubscribe<StageStartedEvent>(OnStageStarted);
            EventBus.Unsubscribe<StageTimerUpdatedEvent>(OnTimerUpdated);
        }
    }
}
