using Percent111.ProjectNS.Item;
using UnityEngine;

namespace Percent111.ProjectNS.DI
{
    /// <summary>
    /// ItemSpawner 설정 (아이템 드롭 설정)
    /// </summary>
    [CreateAssetMenu(fileName = "ItemSpawnerSettings", menuName = "ProjectNS/Settings/ItemSpawnerSettings")]
    public class ItemSpawnerSettings : ScriptableObject
    {
        [Header("Shield Item")]
        public ShieldItem shieldItemPrefab;
        
        [Range(0f, 1f)]
        [Tooltip("방어막 아이템 드롭 확률")]
        public float dropChance = 0.2f;
        
        public int preLoadCount = 10;
    }
}
