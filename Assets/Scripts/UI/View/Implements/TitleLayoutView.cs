using Aftertime.SecretSome;
using Aftertime.SecretSome.UI;
using UnityEngine;

namespace MyNamespace
{
    public class TitleLayoutView : View
    {
        public ExtendedButton StartButton => _startButton;
        [SerializeField] private ExtendedButton _startButton;
    }
   
}