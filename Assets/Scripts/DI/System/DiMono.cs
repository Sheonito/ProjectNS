using UnityEngine;

namespace Percent111.ProjectNS.DI.System
{
    public class DIMono : MonoBehaviour
    {
        protected virtual void Awake()
        {
            DIResolver.Inject(this);
        }
    }   
}