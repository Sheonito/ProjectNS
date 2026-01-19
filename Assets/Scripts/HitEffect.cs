using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Percent111.ProjectNS.Event;

namespace Percent111.ProjectNS.Effect
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitEffect : MonoBehaviour
    {
        [Header("Frames")]
        [SerializeField] private Sprite[] frames;

        [Header("Playback")]
        [SerializeField] private float duration = 0.1f;

        private SpriteRenderer spriteRenderer;
        private CancellationTokenSource cts;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            Play().Forget();
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void OnDestroy()
        {
            Cancel();
        }

        // Pool 반환용 초기화
        public void ResetForPool()
        {
            Cancel();
            if (spriteRenderer != null && frames != null && frames.Length > 0)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void Cancel()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        public async UniTaskVoid Play()
        {
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning("HitEffect: No frames assigned.");
                return;
            }

            Cancel();
            cts = new CancellationTokenSource();

            float frameTime = duration / frames.Length;

            try
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    spriteRenderer.sprite = frames[i];
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(frameTime),
                        DelayType.DeltaTime,
                        PlayerLoopTiming.Update,
                        cts.Token
                    );
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // 이벤트 발행하여 Pool로 반환 요청
            EventBus.Publish(this, new HitEffectReturnEvent(this));
        }
    }

    // HitEffect 반환 요청 이벤트
    public class HitEffectReturnEvent : IEvent
    {
        public HitEffect Effect { get; private set; }

        public HitEffectReturnEvent(HitEffect effect)
        {
            Effect = effect;
        }

        public Type GetPublishType()
        {
            return typeof(HitEffect);
        }
    }
}
