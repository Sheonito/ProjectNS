using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

[RequireComponent(typeof(SpriteRenderer))]
public class HitEffect : MonoBehaviour
{
    [Header("Frames")]
    [SerializeField] private Sprite[] frames;

    [Header("Playback")]
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private bool destroyOnFinish = true;

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
            Debug.LogWarning("HitEffectSpriteAnimatorUniTask: No frames assigned.");
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

        if (destroyOnFinish)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}