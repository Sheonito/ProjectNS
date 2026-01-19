using Percent111.ProjectNS.Enemy;
using UnityEngine;

namespace Percent111.ProjectNS.DI
{
    /// <summary>
    /// BattleManager 설정 (Player, Enemy, Projectile 프리팹 및 풀 설정)
    /// </summary>
    [CreateAssetMenu(fileName = "BattleManagerSettings", menuName = "ProjectNS/Settings/BattleManagerSettings")]
    public class BattleManagerSettings : ScriptableObject
    {
        [Header("Player")]
        public GameObject playerPrefab;

        [Header("Enemy")]
        public GameObject meleeEnemyPrefab;
        public GameObject rangedEnemyPrefab;
        public int enemyPreLoadCount = 10;

        [Header("Projectile")]
        public Projectile projectilePrefab;
        public int projectilePreLoadCount = 20;
    }
}
