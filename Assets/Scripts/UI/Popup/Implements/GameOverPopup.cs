using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Percent111.ProjectNS.UI
{
    // 게임 오버 팝업
    public class GameOverPopup : PopupBase,IRestartable
    {
        [Header("Buttons")]
        [SerializeField] private Button _restartButton;

        protected override void Awake()
        {
            base.Awake();

            // 버튼 이벤트 등록
            _restartButton?.onClick.AddListener(ReStart);
        }

        public void ReStart()
        {
            PopupManager.Instance.Hide<GameOverPopup>();
            EventBus.Publish(this, new GameRestartEvent());
        }

        private void OnDestroy()
        {
            _restartButton?.onClick.RemoveListener(ReStart);
        }
    }
}
