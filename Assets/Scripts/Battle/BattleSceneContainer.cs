using System.Collections.Generic;
using Percent111.ProjectNS.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    /// 전투 씬 컨테이너 (씬에 배치된 오브젝트 참조)
    public class BattleSceneContainer : MonoBehaviour
    {
        [Header("Spawn Points")]
        public Transform playerSpawnPoint;
        public List<Transform> enemySpawnPoints;

        [Header("Cinemachine")]
        public CinemachineCamera cinemachineCamera;

        [Header("UI")]
        public StageUI stageUI;
    }
}
