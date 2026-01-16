using Percent111.ProjectNS.UI.Button.System;
using System;
namespace Percent111.ProjectNS.UI.Popup.System
{
    public interface IChoicePopup
    {
        public void ShowSelect(string selectTitle);
        public void RegisterSelectEvent(string selectTitle, Action onClick);
        public ExtendedButton GetSelectButton(string selectTitle);
    }
}


