using Percent111.ProjectNS.Battle;
using Percent111.ProjectNS.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Percent111.ProjectNS.UI
{
    // 게임 클리어 팝업
    public class GameClearPopup : PopupBase, IRestartable
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
            Hide();
            EventBus.Publish(this, new GameRestartEvent());
        }

        private void OnDestroy()
        {
            _restartButton?.onClick.RemoveListener(ReStart);
        }
    }
}
