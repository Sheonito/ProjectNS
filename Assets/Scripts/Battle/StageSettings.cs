using System.Collections.Generic;
using UnityEngine;

namespace Percent111.ProjectNS.Battle
{
    // 스테이지 설정 (ScriptableObject)
    [CreateAssetMenu(fileName = "StageSettings", menuName = "ProjectNS/Battle/StageSettings")]
    public class StageSettings : ScriptableObject
    {
        [Header("Stage")]
        public int maxStage = 6;

        [Header("Enemy Count")]
        public int baseEnemyCount = 3;
        public int enemyIncreasePerStage = 2;

        [Header("Enemy Type Ratio")]
        [Range(0f, 1f)]
        public float rangedChanceIncreasePerStage = 0.1f;

        [Header("Separation (적끼리 밀어내기)")]
        public float separationRadius = 1f;
        public float separationForce = 5f;
        public int separationUpdateInterval = 3;
    }
}
