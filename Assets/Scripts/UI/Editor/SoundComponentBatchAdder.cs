#if UNITY_EDITOR
using Aftertime.SecretSome.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Aftertime.SecretSome.UI.Editor
{
    public static class SoundComponentBatchAdder
    {
        [MenuItem("Tools/UI/Add SoundComponent To Buttons In Open Scenes")]
        private static void AddSoundComponentToButtonsInOpenScenes()
        {
            // Add SoundComponent to all button objects in every open scene.
            int totalAdded = 0;
            int sceneCount = SceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                totalAdded += AddSoundComponentToScene(scene);
            }

            Debug.Log(string.Format("SoundComponent: Added to {0} GameObjects in open scenes.", totalAdded));
        }

        private static int AddSoundComponentToScene(Scene scene)
        {
            // Add SoundComponent to buttons within a scene.
            if (scene.isLoaded == false)
                return 0;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            int addedCount = 0;

            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject rootObject = rootObjects[i];
                if (rootObject == null)
                    continue;

                Button[] buttons = rootObject.GetComponentsInChildren<Button>(true);
                addedCount += AddSoundComponentToButtons(buttons);
            }

            if (addedCount > 0)
                EditorSceneManager.MarkSceneDirty(scene);

            return addedCount;
        }

        private static int AddSoundComponentToButtons(Button[] buttons)
        {
            // Add SoundComponent to button objects that do not have it yet.
            if (buttons == null)
                return 0;

            int addedCount = 0;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                SoundComponent existing = button.GetComponent<SoundComponent>();
                if (existing != null)
                    continue;

                Undo.AddComponent<SoundComponent>(button.gameObject);
                addedCount++;
            }

            return addedCount;
        }
    }
}
#endif
