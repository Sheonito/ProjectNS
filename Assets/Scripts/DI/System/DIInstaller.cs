using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Percent111.ProjectNS.DI.Implements;

namespace Percent111.ProjectNS.DI.System
{
    public class DIInstaller : MonoBehaviour
    {
        
        public void Install()
        {
            new SampleContainer(null,null);
        }
    }
}
