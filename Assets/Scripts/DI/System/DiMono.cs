using UnityEngine;

namespace Percent111.ProjectNS.DI
{
    public class DIMono : MonoBehaviour
    {
        protected virtual void Awake()
        {
            DIResolver.Inject(this);
        }
    }   
}