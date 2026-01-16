using System;
using System.Collections.Generic;
using Percent111.ProjectNS.UI.Popup.System;
using Percent111.ProjectNS.UI.View.System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Percent111.ProjectNS.UI.Layout.System
{
    [RequireComponent(typeof(View.System.View))]
    public class LayoutBase : MonoBehaviour

    {
        public event Action onShow;

        public event Action onHide;

        public bool IsOn { get; private set; }


        public float FadeDuration => _fadeDuration;

        [SerializeField] protected float _fadeDuration;

        [SerializeField] private List<PopupBase> _popups;


        protected View.System.View _view;

        private bool _isInit;


        protected virtual void Awake()

        {
            Init();
        }


        private void Init()

        {
            if (_isInit)

                return;


            _isInit = true;


            _view = GetComponent<View.System.View>();
        }

        public virtual void Show(bool isShowImmediately = false)

        {
            IsOn = true;

            float duration = isShowImmediately ? 0 : _fadeDuration;


            SetInteractable(true);

            _view.FadeCanvasGroup(1, duration);


            onShow?.Invoke();
        }


        public virtual void Show(float duration)

        {
            float originDuration = _fadeDuration;

            _fadeDuration = duration;

            Show();

            _fadeDuration = originDuration;
        }


        public virtual void Hide(bool isHideImmediately = false)

        {
            IsOn = false;

            float duration = isHideImmediately ? 0 : _fadeDuration;


            SetInteractable(false);

            _view.FadeCanvasGroup(0, duration);


            PopupManager.Instance.HideAllInLayout(this);


            onHide?.Invoke();
        }


        public virtual void Hide(float duration)

        {
            float originDuration = _fadeDuration;

            _fadeDuration = duration;

            Hide();

            _fadeDuration = originDuration;
        }


        public virtual async UniTask HideAsync(bool isHideImmediately = false)

        {
            Hide(isHideImmediately);


            float duration = isHideImmediately ? 0 : _fadeDuration;

            await UniTask.WaitForSeconds(duration);
        }


        public virtual void SetInteractable(bool enable)

        {
            _view.SetInteractable(enable);

            _view.SetBlocksRaycasts(enable);
        }
    }
}