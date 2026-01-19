using System;
using Percent111.ProjectNS.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunner : MonoBehaviour
{
    private void Awake()
    {
        SceneEntryManager.Instance.Additive(GameScene.Global);
    }
}
