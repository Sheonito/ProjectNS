using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Percent111.ProjectNS.DI
{
    public class DIInstaller : MonoBehaviour
    {
        
        public void Install()
        {
            new SampleContainer(null, null);
        }
    }
}
