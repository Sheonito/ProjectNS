using System.Collections.Generic;
using Aftertime.StorylineEngine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Waving.BlackSpin.Common;

namespace Waving.Scene
{
    using Scene = UnityEngine.SceneManagement.Scene;
    using SceneInfo = SceneTable.SceneInfo;

    public enum GameScene
    {
        Title,
        InGame,
    }

    public class SceneEntryManager : SingletonMonoBehaviour<SceneEntryManager>
    {
        [SerializeField] private SceneTable _sceneTable;

        private Dictionary<SceneInfo, ISceneEntry> _entries = new();

        public GameScene CurrentSceneType { get; private set; }

        public ISceneEntry CurrentSceneEntry
        {
            get
            {
                SceneInfo sceneInfo = _sceneTable.GetSceneInfo(CurrentSceneType);
                return _entries[sceneInfo];
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            _sceneTable = GetSceneTable();
            _entries = CreateSceneEntries();
        }

        public ISceneEntry GetSceneEntry(GameScene sceneType)
        {
            return _entries[_sceneTable.GetSceneInfo(sceneType)];
        }

        private SceneTable GetSceneTable()
        {
            string path = Define.SceneTablePath;
            SceneTable sceneTable = Resources.Load<SceneTable>(path);
            return sceneTable;
        }

        private Dictionary<SceneInfo, ISceneEntry> CreateSceneEntries()
        {
            TitleSceneEntry titleEntry = new TitleSceneEntry();
            InGameSceneEntry inGameEntry = new InGameSceneEntry();

            Dictionary<SceneInfo, ISceneEntry> entries = new Dictionary<SceneInfo, ISceneEntry>();
            SceneInfo titleInfo = _sceneTable.GetSceneInfo(GameScene.Title);
            SceneInfo ingameInfo = _sceneTable.GetSceneInfo(GameScene.InGame);
            entries.Add(titleInfo, titleEntry);
            entries.Add(ingameInfo, inGameEntry);
            return entries;
        }


        public async UniTask ChangeScene(GameScene sceneType)
        {
            SceneInfo currentSceneInfo = _sceneTable.GetSceneInfo(CurrentSceneType);
            SceneInfo loadSceneInfo = _sceneTable.GetSceneInfo(sceneType);
            await UnloadScene(currentSceneInfo);
            ExitCurrentSceneEntry();
            
            await LoadSceneAdditive(loadSceneInfo);
            EnterSceneEntry(loadSceneInfo);
        }

        public async UniTask Additive(GameScene sceneType)
        {
            SceneInfo sceneInfo = _sceneTable.GetSceneInfo(sceneType);
            await LoadSceneAdditive(sceneInfo);
            EnterSceneEntry(sceneInfo);
        }

        public async UniTask Remove(GameScene sceneType)
        {
            SceneInfo sceneInfo = _sceneTable.GetSceneInfo(sceneType);
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

        private void EnterSceneEntry(SceneInfo sceneInfo)
        {
            _entries[sceneInfo].OnEnter();
        }

        private void ExitCurrentSceneEntry()
        {
            CurrentSceneEntry.OnExit();
        }
    }
}