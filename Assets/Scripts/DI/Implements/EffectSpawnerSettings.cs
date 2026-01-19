using Percent111.ProjectNS.Effect;
using UnityEngine;

namespace Percent111.ProjectNS.DI
{
    /// <summary>
    /// EffectSpawner 설정 (이펙트 설정)
    /// </summary>
    [CreateAssetMenu(fileName = "EffectSpawnerSettings", menuName = "ProjectNS/Settings/EffectSpawnerSettings")]
    public class EffectSpawnerSettings : ScriptableObject
    {
        [Header("Hit Effect")]
        public HitEffect hitEffectPrefab;
        public int preLoadCount = 20;
    }
}
