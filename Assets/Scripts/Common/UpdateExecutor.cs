using System;
using UnityEngine;

namespace Percent111.ProjectNS.Common
{
    public class UpdateExecutor : MonoBehaviour
    {
        public static Action onUpdate = delegate { };
        
        private void Update()
        {
            onUpdate();
        }
    }
   
}
