using Aftertime.SecretSome.UI.Layout;
using Aftertime.StorylineEngine;
using UnityEngine;
using Waving.BlackSpin.UI;

namespace Waving.Scene
{
    public class TitleSceneEntry : ISceneEntry
    {
        private TitleLayout _titleLayout;
        public void OnEnter()
        {
            if (_titleLayout == null)
                _titleLayout = Object.FindFirstObjectByType<TitleLayout>();
            
            _titleLayout.Show();
            SoundManager.Instance.PlayBGM("Title",0.2f);
        }

        public void OnExit()
        {
            SoundManager.Instance.StopBGM(0.5f);
        }
    }
}