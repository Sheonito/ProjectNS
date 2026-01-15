using System;
using System.Collections.Generic;
using System.Threading;
using Aftertime.SecretSome.UI.Page;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Waving.Scene;

namespace Waving.BlackSpin.UI
{
    public class BattlePage : PageBase
    {
        [SerializeField] private Image _enemyBGImage;
        [SerializeField] private List<Sprite> _enemyBGSpriteList;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private CanvasGroup _messageCanvasGroup;
        
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public override void Show(bool hasDuration)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            base.Show(hasDuration);
            PlayEnemyBG();
            ShowMessage();
        }

        private async void ShowMessage()
        {
            if (InGameSceneEntry._round != 1)
                return;
            
            _messageCanvasGroup.DOKill();
            _messageCanvasGroup.alpha = 0;
            _messageCanvasGroup.DOFade(1, 0.2f);
            _messageText.text = "드디어 새로운 승무원이 왔구나?";
            await UniTask.WaitForSeconds(4);
            _messageText.text = "HIT를 통해 카드를 뽑을 수 있어";
            await UniTask.WaitForSeconds(4);
            _messageText.text = "STAY를 누르면 너의 카드를\n고정할 거야";
            await UniTask.WaitForSeconds(4);
            _messageText.text = "21에 가장 가까운 숫자가\n나오는 사람이 승리야";
            await UniTask.WaitForSeconds(4);
            _messageText.text = "21을 초과하면 패배하니까\n조심하라구";
            await UniTask.WaitForSeconds(4);
            _messageCanvasGroup.DOFade(0, 0.2f);
        }

        public override void Hide(bool hasDuration)
        {
            base.Hide(hasDuration);
            _messageText.DOKill();
            _enemyBGImage.DOFade(0, 1f);
        }

        private void PlayEnemyBG()
        {
            _enemyBGImage.DOFade(1, 1f);
            PlayAnimation().Forget();
        }

        private async UniTaskVoid PlayAnimation()
        {
            int index = 0;

            while (true)
            {
                if (!IsOn)
                {
                    _enemyBGImage.DOFade(0, 0.5f);
                    return;
                }

                _enemyBGImage.sprite = _enemyBGSpriteList[index];

                index++;
                if (index >= _enemyBGSpriteList.Count)
                    index = 0; // Loop

                await UniTask.WaitForSeconds(0.1f).AttachExternalCancellation(_cts.Token);

                if (_cts.IsCancellationRequested)
                    return;
            }
        }
    }
}