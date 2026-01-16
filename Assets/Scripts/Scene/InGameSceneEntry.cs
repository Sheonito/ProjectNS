using Percent111.ProjectNS.DI.System;
using Percent111.ProjectNS.Event;
using UnityEngine;
namespace Percent111.ProjectNS.Scene
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
