using System.Collections.Generic;
using Percent111.ProjectNS.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Percent111.ProjectNS.Scene
{
    using Scene = UnityEngine.SceneManagement.Scene;
    using SceneInfo = SceneTable.SceneInfo;

    public enum GameScene
    {
        Title,
        InGame,
        Global
    }

    public class SceneEntryManager : SingletonMonoBehaviour<SceneEntryManager>
    {
        public SceneTable SceneTable
        {
            get
            {
                if (_SceneTable == null)
                {
                    _SceneTable = Resources.Load<SceneTable>(ResourcesPath.SceneTablePath);
                }

                return _SceneTable;
            }
        }

        private SceneTable _SceneTable;

        public GameScene CurrentSceneType { get; private set; }

        public ISceneEntry CurrentSceneEntry { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += EnterSceneEntry;
        }

        public async UniTask ChangeScene(GameScene sceneType)
        {
            SceneInfo loadSceneInfo = SceneTable.GetSceneInfo(sceneType);
            string loadSceneName = loadSceneInfo.sceneName;
            ExitCurrentSceneEntry();
            SceneManager.LoadScene(loadSceneName);
        }

        public async UniTask Additive(GameScene sceneType)
        {
            SceneInfo sceneInfo = SceneTable.GetSceneInfo(sceneType);
            await LoadSceneAdditive(sceneInfo);
        }

        public async UniTask Remove(GameScene sceneType)
        {
            SceneInfo sceneInfo = SceneTable.GetSceneInfo(sceneType);
            ExitCurrentSceneEntry();
            await UnloadScene(sceneInfo);
        }

        private async UniTask LoadSceneAdditive(SceneInfo sceneInfo)
        {
            string sceneName = sceneInfo.sceneName;
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            CurrentSceneType = sceneInfo.gameScene;
        }

        private async UniTask UnloadScene(SceneInfo sceneInfo)
        {
            string sceneName = sceneInfo.sceneName;
            Scene scene = SceneManager.GetSceneByName(sceneName);
            await SceneManager.UnloadSceneAsync(scene);
        }
        
        private void EnterSceneEntry(Scene scene, LoadSceneMode mode)
        {
            ISceneEntry entry = null;
            entry = FindAnyObjectByType<InGameSceneEntry>();

            string sceneName = scene.name;
            if (sceneName == nameof(GameScene.Title))
            {
                entry = FindAnyObjectByType<TitleSceneEntry>();
            }
            else if (sceneName == nameof(GameScene.InGame))
            {
                entry = FindAnyObjectByType<InGameSceneEntry>();
            }
            
            entry.OnEnter();
        }

        private void ExitCurrentSceneEntry()
        {
            if (CurrentSceneEntry != null)
                CurrentSceneEntry.OnExit();
        }
    }
}