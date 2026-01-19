using Percent111.ProjectNS.Scene;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    public class GameRunner : MonoBehaviour
    {
        private void Awake()
        {
            SceneEntryManager.Instance.Additive(GameScene.Global);
        }
    }
}
