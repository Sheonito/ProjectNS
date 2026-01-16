namespace Percent111.ProjectNS.UI.Page.System
{
    public interface IPagable
    {
        public T GetPage<T>() where T : PageBase;
        public void ShowPage<T>(bool hasDuration) where T : PageBase;
        public void HidePage<T>(bool hasDuration) where T : PageBase;
        
    }
   
}
