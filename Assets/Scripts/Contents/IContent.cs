using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Percent111.ProjectNS.Contents
{
    public enum ContentState
    {
        Stop,
        Pause,
        Running
    }

    public interface IContent
    {
        public ContentState State { get; }

        public void StartContent();
        public void PauseContent();
        public UniTask StartContentAsync();
        public UniTask StopContentAsync();
        public void StopContent();
        public void ResumeContent();
    }
}

