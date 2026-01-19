using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Percent111.ProjectNS.Scene
{
    public class TitleSceneEntry : MonoBehaviour, ISceneEntry
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Image _dimImage;
        [SerializeField] private CanvasGroup _textCanvasGroup;

        private const float DimFadeDuration = 0.5f;

        private void Awake()
        {
            RegisterButtonEvents();
        }

        private void RegisterButtonEvents()
        {
            _startButton.onClick.AddListener(StartGame);
            _exitButton.onClick.AddListener(Application.Quit);
        }

        private void StartGame()
        {
            _startButton.interactable = false;
            _exitButton.interactable = false;
            
            _textCanvasGroup.DOFade(0, DimFadeDuration);
            _dimImage.DOFade(0, DimFadeDuration).onComplete += () =>
            {
                SceneEntryManager.Instance.ChangeScene(GameScene.InGame);
            };
        }

        public void OnEnter()
        {
            _startButton.interactable = true;
            _exitButton.interactable = true;
        }

        public void OnExit()
        {
        }
    }
}