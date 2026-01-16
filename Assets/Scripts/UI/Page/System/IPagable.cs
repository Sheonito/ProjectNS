namespace Percent111.ProjectNS.UI
{
    public interface IPagable
    {
        public T GetPage<T>() where T : PageBase;
        public void ShowPage<T>(bool hasDuration) where T : PageBase;
        public void HidePage<T>(bool hasDuration) where T : PageBase;
        
    }
   
}
