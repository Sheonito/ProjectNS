using System;

namespace Percent111.ProjectNS.UI
{
    public interface IChoicePopup
    {
        public void ShowSelect(string selectTitle);
        public void RegisterSelectEvent(string selectTitle, Action onClick);
        public ExtendedButton GetSelectButton(string selectTitle);
    }
}


