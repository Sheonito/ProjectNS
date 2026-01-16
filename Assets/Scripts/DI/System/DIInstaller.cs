using System.Collections.Generic;
using Aftertime.SecretSome.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Waving.BlackSpin.DI;

namespace Waving.Di
{
    public class DIInstaller : MonoBehaviour
    {
        
        public void Install()
        {
            new SampleContainer(null,null);
        }
    }
}