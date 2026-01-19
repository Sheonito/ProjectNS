using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Percent111.ProjectNS.UI
{
    // 게임 오버 팝업
    public class GameOverPopup : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _restartButton;

        protected override void Awake()
        {
            base.Awake();

            // 버튼 이벤트 등록
            _restartButton?.onClick.AddListener(OnRestartClicked);
        }

        // 재시작 버튼 클릭
        private void OnRestartClicked()
        {
            Hide();
            EventBus.Publish(this, new GameRestartEvent());
        }

        private void OnDestroy()
        {
            _restartButton?.onClick.RemoveListener(OnRestartClicked);
        }
    }
}
