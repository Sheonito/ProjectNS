using UnityEngine;
using Waving.BlackSpin.Event;
using Waving.Common.Event;
using Waving.Di;

namespace Waving.Scene
{
    public class InGameSceneEntry : ISceneEntry
    {
        private DIInstaller _diInstaller;

        public void OnEnter()
        {
            InstallDI();
            Init();
            RegisterEvents();
        }

        public void OnExit()
        {
        }

        private void InstallDI()
        {
            if (_diInstaller == null)
                _diInstaller = Object.FindFirstObjectByType<DIInstaller>();
            _diInstaller.Install();
        }

        private void Init()
        {
        }

        private void RegisterEvents()
        {
            EventBus.Subscribe<SampleEvent>(DoSampleEvent);
        }

        private void DoSampleEvent(SampleEvent sampleEvent)
        {
        }
    }
}